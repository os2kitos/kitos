using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class OrgUnitService : IOrgUnitService
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<OrganizationRight> _orgRightRepository;
        private readonly IAdminService _adminService;

        public OrgUnitService(IGenericRepository<OrganizationUnit> orgUnitRepository, IGenericRepository<OrganizationRight> orgRightRepository, IAdminService adminService)
        {
            _orgUnitRepository = orgUnitRepository;
            _orgRightRepository = orgRightRepository;
            _adminService = adminService;
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
    }
}