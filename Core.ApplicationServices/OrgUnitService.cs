using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices
{
    public class OrgUnitService : IOrgUnitService
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<OrganizationRight> _orgRightRepository;

        public OrgUnitService(IGenericRepository<OrganizationUnit> orgUnitRepository, IGenericRepository<OrganizationRight> orgRightRepository)
        {
            _orgUnitRepository = orgUnitRepository;
            _orgRightRepository = orgRightRepository;
        }

        public ICollection<OrganizationUnit> GetByUser(User user)
        {
            List<OrganizationUnit> units;

            if (user.IsGlobalAdmin)
            {
                units = _orgUnitRepository.Get().ToList();
            }
            else
            {
                //add the OrgUnits that the user is directly connected to, through OrgRights
                units = user.OrganizationRights.Select(orgRight => orgRight.Object).ToList();

                //add the OrgUnits that the user is indirectly connected to, through an Admin role on an organization
                foreach (var adminRights in user.AdminRights)
                {
                    units.Add(adminRights.Object.OrgUnits.First());
                }

            }

            var roots = units.Select(GetRoot);
            return roots.Distinct().ToList();
        }

        public OrganizationUnit GetRoot(OrganizationUnit unit)
        {
            var whereWeStarted = unit;

            while (unit.Parent != null)
            {
                unit = unit.Parent;

                //did we get a loop?
                if(unit.Id == whereWeStarted.Id) throw new Exception("Loop in Organization Units");
            }

            return unit;
        }

        public ICollection<OrganizationUnit> GetSubTree(int orgUnitId)
        {
            var orgUnit = _orgUnitRepository.GetByKey(orgUnitId);

            return GetSubTree(orgUnit);
        }

        public ICollection<OrganizationUnit> GetSubTree(OrganizationUnit unit)
        {
            var unreached = new Queue<OrganizationUnit>();
            var reached = new List<OrganizationUnit>();

            unreached.Enqueue(unit);
            while (unreached.Count > 0)
            {
                var orgUnit = unreached.Dequeue();

                reached.Add(orgUnit);

                foreach (var child in orgUnit.Children)
                {
                    unreached.Enqueue(child);
                }
            }

            return reached;
        }

        public bool IsAncestorOf(OrganizationUnit unit, OrganizationUnit ancestor)
        {
            if (unit == null || ancestor == null) return false;

            do
            {
                if (unit.Id == ancestor.Id) return true;

                unit = unit.Parent;

            } while (unit != null);

            return false;
        }

        public bool IsAncestorOf(int unitId, int ancestorId)
        {
            var unit = _orgUnitRepository.GetByKey(unitId);
            var ancestor = _orgUnitRepository.GetByKey(ancestorId);

            return IsAncestorOf(unit, ancestor);
        }

        public bool HasWriteAccess(User user, int orgUnitId)
        {
            var orgUnit = _orgUnitRepository.GetByKey(orgUnitId);

            return HasWriteAccess(user, orgUnit);
        }

        public bool HasWriteAccess(User user, OrganizationUnit unit)
        {
            //if user is global admin or local admin, user has write access
            if (user.IsGlobalAdmin) return true;

            if (user.AdminRights.Select(r => r.Object_Id == unit.Organization_Id).Any()) return true;

            // check all rights for the user on this org unit,
            // as well as every ancestor org unit
            // if we find a unit with write access, we return it
            do
            {
                //this is to avoid the 'access to modified closure' warning
                var currUnit = unit;

                var writeRights =
                    _orgRightRepository.Get(
                        right => right.User_Id == user.Id && right.Object_Id == currUnit.Id && right.Role.HasWriteAccess).ToList();

                if (writeRights.Any()) return true;

                unit = currUnit.Parent;

            } while (unit != null);

            return false;
        }
    }
}