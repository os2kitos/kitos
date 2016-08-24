using System;
using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainServices;
using System.Linq;

namespace Core.ApplicationServices
{
    public class OrgUnitService : IOrgUnitService
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<TaskUsage> _taskUsageRepository;

        public OrgUnitService(IGenericRepository<OrganizationUnit> orgUnitRepository, IGenericRepository<TaskUsage> taskUsageRepository)
        {
            _orgUnitRepository = orgUnitRepository;
            _taskUsageRepository = taskUsageRepository;
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

        public void Delete(int id)
        {
            // delete task usages
            var taskUsages = _taskUsageRepository.Get(x => x.OrgUnitId == id);
            foreach (var taskUsage in taskUsages)
            {
                _taskUsageRepository.DeleteByKey(taskUsage.Id);
            }
            _taskUsageRepository.Save();

            // http://stackoverflow.com/questions/15226312/entityframewok-how-to-configure-cascade-delete-to-nullify-foreign-keys
            // when children are loaded into memory the foreign key is correctly set to null on children when deleted
            var orgUnit = _orgUnitRepository.Get(x => x.Id == id, null, $"{nameof(OrganizationUnit.DefaultUsers)}").FirstOrDefault();

            // attach children to parent of this instance to avoid orphans
            // parent id will never be null because users aren't allowed to delete the root node
            foreach (var child in orgUnit.Children)
            {
                child.ParentId = orgUnit.ParentId;
            }

            _orgUnitRepository.Delete(orgUnit);
            _orgUnitRepository.Save();
        }
    }
}
