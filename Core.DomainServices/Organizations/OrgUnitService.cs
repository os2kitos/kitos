using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;

namespace Core.DomainServices.Organizations
{
    public class OrgUnitService : IOrgUnitService
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<ItSystemUsageOrgUnitUsage> _itSystemUsageOrgUnitUsageRepository;
        private readonly IGenericRepository<TaskUsage> _taskUsageRepository;
        private readonly IGenericRepository<ItProject> _itProjectRepository;
        private readonly IGenericRepository<ItProjectOrgUnitUsage> _itProjectOrgUnitUsageRepository;

        public OrgUnitService(IGenericRepository<OrganizationUnit> orgUnitRepository, IGenericRepository<TaskUsage> taskUsageRepository, IGenericRepository<ItSystemUsageOrgUnitUsage> itSystemUsageOrgUnitUsageRepository, IGenericRepository<ItProject> itProjectRepository, IGenericRepository<ItProjectOrgUnitUsage> itProjectOrgUnitUsageRepository)
        {
            _orgUnitRepository = orgUnitRepository;
            _taskUsageRepository = taskUsageRepository;
            _itSystemUsageOrgUnitUsageRepository = itSystemUsageOrgUnitUsageRepository;
            _itProjectRepository = itProjectRepository;
            _itProjectOrgUnitUsageRepository = itProjectOrgUnitUsageRepository;
        }

        public OrganizationUnit GetRoot(OrganizationUnit unit)
        {
            var whereWeStarted = unit;

            while (unit.Parent != null)
            {
                unit = unit.Parent;

                //did we get a loop?
                if (unit.Id == whereWeStarted.Id) throw new Exception("Loop in Organization Units");
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


            // Remove OrgUnit from ItSystemUsages
            var itSystemUsageOrgUnitUsages = _itSystemUsageOrgUnitUsageRepository.Get(x => x.OrganizationUnitId == id);
            foreach (var itSystemUsage in itSystemUsageOrgUnitUsages)
            {
                if (itSystemUsage.ResponsibleItSystemUsage != null)
                {
                    throw new ArgumentException($"OrganizationUnit is ResponsibleOrgUnit for ItSystemUsage: {itSystemUsage.ItSystemUsageId}");
                }

                _itSystemUsageOrgUnitUsageRepository.Delete(itSystemUsage);

            }
            _itSystemUsageOrgUnitUsageRepository.Save();

            // Remove OrgUnit from ItProjects
            var itProjects = _itProjectRepository.Get(project => project.UsedByOrgUnits.Any(x => x.OrganizationUnitId == id));
            foreach (var itproject in itProjects)
            {
                if (itproject.ResponsibleUsage != null && itproject.ResponsibleUsage.OrganizationUnitId == id)
                {
                    throw new ArgumentException($"OrganizationUnit is ResponsibleOrgUnit for ItProject: {itproject.Id}");
                }
                var itprojectOrgUnitUsage = itproject.UsedByOrgUnits.Single(x => x.OrganizationUnitId == id);
                _itProjectOrgUnitUsageRepository.Delete(itprojectOrgUnitUsage);
            }
            _itProjectOrgUnitUsageRepository.Save();

            var orgUnit = _orgUnitRepository.GetByKey(id);

            // attach children to parent of this instance to avoid orphans
            // parent id will never be null because users aren't allowed to delete the root node
            foreach (var child in orgUnit.Children)
            {
                child.ParentId = orgUnit.ParentId;
            }

            _orgUnitRepository.DeleteWithReferencePreload(orgUnit);
            _orgUnitRepository.Save();
        }

        public IQueryable<OrganizationUnit> GetOrganizationUnits(Organization organization)
        {
            return _orgUnitRepository.AsQueryable().ByOrganizationId(organization.Id);
        }
    }
}
