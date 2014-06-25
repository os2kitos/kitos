using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
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

        public HttpResponseMessage Get(int orgUnitId, [FromUri] PagingModel<TaskUsage> pagingModel)
        {
            return Get(orgUnitId, false, pagingModel);
        }

        public HttpResponseMessage Get(int orgUnitId, bool onlyStarred, [FromUri] PagingModel<TaskUsage> pagingModel)
        {
            try
            {
                pagingModel.Where(usage => usage.OrgUnitId == orgUnitId);

                if (onlyStarred) pagingModel.Where(usage => usage.Starred);

                var usages = Page(Repository.AsQueryable(), pagingModel).ToList();

                var dtos = new List<TaskUsageNestedDTO>();

                foreach (var taskUsage in usages)
                {
                    var dto = Map<TaskUsage, TaskUsageNestedDTO>(taskUsage);
                    dto.HasWriteAccess = HasWriteAccess(taskUsage, KitosUser);
                    dto.SystemUsages = AssociatedSystemUsages(taskUsage);
                    dto.Projects = AssociatedProjects(taskUsage);
                    dtos.Add(dto);
                }
                
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        //public HttpResponseMessage GetProjects(int id, bool? projects)
        //{
        //    try
        //    {
        //        var usage = Repository.GetByKey(id);

        //        var theProjects = usage.TaskRef.ItProjects.Where(p => p.OrganizationId == usage.OrgUnit.OrganizationId);
        //        var dtos = Map<IEnumerable<ItProject>, IEnumerable<ItProjectSimpleDTO>>(theProjects);

        //        return Ok(dtos);
        //    }
        //    catch (Exception e)
        //    {
        //        return Error(e);
        //    }
        //}

        //public HttpResponseMessage GetSystems(int id, bool? systems)
        //{
        //    try
        //    {
        //        var taskUsage = Repository.GetByKey(id);


        //        var indirectUsages =
        //            taskUsage.TaskRef.ItSystems.SelectMany(system => system.Usages)
        //                     .Where(usage => usage.OrganizationId == taskUsage.OrgUnit.OrganizationId);

        //        var directUsages =
        //            taskUsage.TaskRef.ItSystemUsages.Where(
        //                usage => usage.OrganizationId == taskUsage.OrgUnit.OrganizationId);

        //        var allUsages = indirectUsages.Union(directUsages);

        //        var dtos = Map<IEnumerable<ItSystemUsage>, IEnumerable<ItSystemUsageSimpleDTO>>(allUsages);

        //        return Ok(dtos);
        //    }
        //    catch (Exception e)
        //    {
        //        return Error(e);
        //    }
        //}

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

        private IEnumerable<ItSystemUsageSimpleDTO> AssociatedSystemUsages(TaskUsage taskUsage)
        {
            var indirectUsages =
                    taskUsage.TaskRef.ItSystems.SelectMany(system => system.Usages)
                             .Where(usage => usage.OrganizationId == taskUsage.OrgUnit.OrganizationId);

            var directUsages =
                taskUsage.TaskRef.ItSystemUsages.Where(
                    usage => usage.OrganizationId == taskUsage.OrgUnit.OrganizationId);

            var allUsages = indirectUsages.Union(directUsages);

            return Map<IEnumerable<ItSystemUsage>, IEnumerable<ItSystemUsageSimpleDTO>>(allUsages);
        }

        private IEnumerable<ItProjectSimpleDTO> AssociatedProjects(TaskUsage taskUsage)
        {
            var theProjects = taskUsage.TaskRef.ItProjects.Where(p => p.OrganizationId == taskUsage.OrgUnit.OrganizationId);
            return Map<IEnumerable<ItProject>, IEnumerable<ItProjectSimpleDTO>>(theProjects);
        } 
    }
}
