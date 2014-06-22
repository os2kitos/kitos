using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class TaskUsageController : GenericHierarchyApiController<TaskUsage, TaskUsageDTO>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;

        public TaskUsageController(IGenericRepository<TaskUsage> repository, IGenericRepository<OrganizationUnit> orgUnitRepository) 
            : base(repository)
        {
            _orgUnitRepository = orgUnitRepository;
        }

        public HttpResponseMessage Get(int orgUnitId)
        {
            return Get(orgUnitId, false);
        }

        public HttpResponseMessage Get(int orgUnitId, bool onlyStarred)
        {
            try
            {
                var usages = Repository.Get(u => u.OrgUnitId == orgUnitId);

                if (onlyStarred) usages = usages.Where(u => u.Starred);

                var dtos = Map<IEnumerable<TaskUsage>, IEnumerable<TaskUsageNestedDTO>>(usages);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetProjects(int id, bool? projects)
        {
            try
            {
                var usage = Repository.GetByKey(id);

                var theProjects = usage.TaskRef.ItProjects.Where(p => p.OrganizationId == usage.OrgUnit.OrganizationId);
                var dtos = Map<IEnumerable<ItProject>, IEnumerable<ItProjectSimpleDTO>>(theProjects);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetSystems(int id, bool? systems)
        {
            try
            {
                var taskUsage = Repository.GetByKey(id);


                var indirectUsages =
                    taskUsage.TaskRef.ItSystems.SelectMany(system => system.Usages)
                             .Where(usage => usage.OrganizationId == taskUsage.OrgUnit.OrganizationId);

                var directUsages =
                    taskUsage.TaskRef.ItSystemUsages.Where(
                        usage => usage.OrganizationId == taskUsage.OrgUnit.OrganizationId);

                var allUsages = indirectUsages.Union(directUsages);

                var dtos = Map<IEnumerable<ItSystemUsage>, IEnumerable<ItSystemUsageSimpleDTO>>(allUsages);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        protected override TaskUsage PostQuery(TaskUsage item)
        {
            var orgUnit = _orgUnitRepository.GetByKey(item.OrgUnitId);

            if (orgUnit.ParentId != null)
            {
                var parentUsage = orgUnit.Parent.TaskUsages.First(usage => usage.TaskRefId == item.TaskRefId);
                item.Parent = parentUsage;
            }

            return base.PostQuery(item);
        }
        
        public override HttpResponseMessage Put(int id, JObject jObject)
        {
            return NotAllowed();
        }
    }
}
