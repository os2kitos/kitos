using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices
{
    public class OrgUnitService : IOrgUnitService
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;

        public OrgUnitService(IGenericRepository<OrganizationUnit> orgUnitRepository)
        {
            _orgUnitRepository = orgUnitRepository;
        }

        public ICollection<OrganizationUnit> GetByUser(User user)
        {
            //add the OrgUnits that the user is directly connected to, through OrgRights
            var units = user.OrganizationRights.Select(orgRight => orgRight.Object).ToList();

            //add the OrgUnits that the user is indirectly connected to, through an Admin role on an organization
            foreach (var adminRights in user.AdminRights)
            {
                units.Add(adminRights.Object.OrgUnits.First());
            }

            var roots = units.Select(GetRoot);
            return roots.Distinct().ToList();
        }

        public OrganizationUnit GetRoot(OrganizationUnit unit)
        {
            //TODO: this will fuck up if there's a loop!
            while (unit.Parent != null) unit = unit.Parent;

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

        public bool HasWriteAccess(User user, int orgUnitId)
        {
            var orgUnit = _orgUnitRepository.GetByKey(orgUnitId);

            return HasWriteAccess(user, orgUnit);
        }

        public bool HasWriteAccess(User user, OrganizationUnit unit)
        {
            return true; //TODO
        }
    }
}