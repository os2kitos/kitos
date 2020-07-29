using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class OrganizationUnitController : GenericHierarchyApiController<OrganizationUnit, OrgUnitDTO>
    {
        private readonly IOrgUnitService _orgUnitService;
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IGenericRepository<TaskUsage> _taskUsageRepository;

        public OrganizationUnitController(
            IGenericRepository<OrganizationUnit> repository,
            IOrgUnitService orgUnitService,
            IGenericRepository<TaskRef> taskRepository,
            IGenericRepository<TaskUsage> taskUsageRepository)
            : base(repository)
        {
            _orgUnitService = orgUnitService;
            _taskRepository = taskRepository;
            _taskUsageRepository = taskUsageRepository;
        }

        /// <summary>
        /// Returns every OrganizationUnit that the user can select as the default unit
        /// </summary>
        /// <param name="byUser">Routing qualifier</param>
        /// <returns></returns>
        public HttpResponseMessage GetByUser(bool? byUser, int organizationId)
        {
            try
            {
                var orgUnits = Repository.Get(x => x.Rights.Any(y => y.UserId == KitosUser.Id) && x.OrganizationId == organizationId).SelectNestedChildren(x => x.Children).ToList();

                orgUnits = orgUnits
                    .Distinct()
                    .Where(AllowRead)
                    .ToList();

                return Ok(Map<IEnumerable<OrganizationUnit>, IEnumerable<OrgUnitSimpleDTO>>(orgUnits));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [CacheApiResponse(DurationInMiliSeconds = 5000)]
        public HttpResponseMessage GetByOrganization(int organization)
        {
            try
            {
                var root = Repository
                    .AsQueryable()
                    .ByOrganizationId(organization)
                    .FirstOrDefault(unit => unit.Parent == null);

                if (root == null) return NotFound();

                if (!AllowRead(root))
                {
                    return Forbidden();
                }

                var item = Map<OrganizationUnit, OrgUnitDTO>(root);

                return Ok(item);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetByOrganizationFlat(int organizationId)
        {
            try
            {
                var orgUnit =
                    Repository
                        .AsQueryable()
                        .ByOrganizationId(organizationId)
                        .AsEnumerable()
                        .Where(AllowRead);

                return Ok(Map(orgUnit));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        {
            try
            {
                JToken jtoken;
                if (obj.TryGetValue("parentId", out jtoken))
                {
                    //TODO: You have to be local or global admin to change parent

                    var parentId = jtoken.Value<int>();

                    //if the new parent is actually a descendant of the item, don't update - this would create a loop!
                    if (_orgUnitService.IsAncestorOf(parentId, id))
                    {
                        return Conflict("OrgUnit loop detected");
                    }
                }

            }
            catch (Exception e)
            {
                return LogError(e);
            }
            return base.Patch(id, organizationId, obj);
        }

        public override HttpResponseMessage Put(int id, int organizationId, JObject jObject)
        {
            return NotAllowed();
        }

        /// <summary>
        /// Returns every task that a given OrgUnit can use. This depends on the task usages of the parent OrgUnit.
        /// For every task returned, possibly a taskUsage is returned too, if the OrgUnit is currently using that task.
        /// </summary>
        /// <param name="id">ID of the OrgUnit</param>
        /// <param name="taskGroup">Optional id to filter by task group</param>
        /// <param name="tasks">Routing qualifier</param>
        /// <param name="pagingModel">Paging options</param>
        /// <returns>List of (task, taskUsage), where the taskUsage might be null</returns>
        public HttpResponseMessage GetAccessibleTasks(int id, int? taskGroup, bool? tasks, [FromUri] PagingModel<TaskRef> pagingModel)
        {
            try
            {
                var orgUnit = Repository.GetByKey(id);

                IQueryable<TaskRef> taskQuery;
                // if the org unit has a parent, only select those tasks that is in use by the parent org unit
                if (orgUnit.ParentId.HasValue)
                {
                    // this is not so good performance wise
                    var orgUnitQueryable = Repository.AsQueryable().Where(unit => unit.Id == id);
                    taskQuery = orgUnitQueryable.SelectMany(u => u.Parent.TaskUsages.Select(usage => usage.TaskRef));

                    // it would have been better with:
                    // pagingModel.Where(taskRef => taskRef.Usages.Any(usage => usage.OrgUnitId == orgUnit.ParentId));
                    // but we cant because of a bug in the mysql connector: http://bugs.mysql.com/bug.php?id=70722
                }
                else
                {
                    taskQuery = _taskRepository.AsQueryable();
                }

                // if a task group is given, only find the tasks in that group and sub groups
                if (taskGroup.HasValue)
                {
                    pagingModel.Where(
                        taskRef =>
                            (taskRef.ParentId.Value == taskGroup.Value ||
                             taskRef.Parent.ParentId.Value == taskGroup.Value) &&
                             !taskRef.Children.Any());
                }
                else
                {
                    // else get all task leaves
                    pagingModel.Where(taskRef => !taskRef.Children.Any());
                }

                pagingModel.WithPostProcessingFilter(AllowRead);
                var theTasks = Page(taskQuery, pagingModel).ToList();

                // convert tasks to DTO containing both the task and possibly also a taskUsage, if that exists
                var dtos = (from taskRef in theTasks
                            let taskUsage = taskRef.Usages.FirstOrDefault(usage => usage.OrgUnitId == id)
                            select new TaskRefUsageDTO()
                            {
                                TaskRef = Map<TaskRef, TaskRefDTO>(taskRef),
                                Usage = Map<TaskUsage, TaskUsageDTO>(taskUsage)
                            }).ToList(); // must call .ToList here else the output will be wrapped in $type,$values

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        /// <summary>
        /// Returns the task usages of a given OrgUnit.
        /// </summary>
        /// <param name="id">ID of the OrgUnit</param>
        /// <param name="taskGroup">Optional id of a taskgroup</param>
        /// <param name="usages">Routing qualifier</param>
        /// <param name="pagingModel">Paging options</param>
        /// <returns>List of (task, taskUsage)</returns>
        public HttpResponseMessage GetTaskUsages(int id, int? taskGroup, bool? usages,
                                                 [FromUri] PagingModel<TaskUsage> pagingModel)
        {
            try
            {
                var usageQuery = _taskUsageRepository.AsQueryable();
                pagingModel.Where(usage => usage.OrgUnitId == id);

                // if a task group is given, only find the tasks in that group and sub groups
                if (taskGroup.HasValue)
                {
                    pagingModel.Where(taskUsage => taskUsage.TaskRef.ParentId.Value == taskGroup.Value ||
                                                   taskUsage.TaskRef.Parent.ParentId.Value == taskGroup.Value);
                }

                pagingModel.WithPostProcessingFilter(AllowRead);
                var theUsages = Page(usageQuery, pagingModel).ToList();

                var dtos = (from usage in theUsages
                            select new TaskRefUsageDTO()
                            {
                                TaskRef = Map<TaskRef, TaskRefDTO>(usage.TaskRef),
                                Usage = Map<TaskUsage, TaskUsageDTO>(usage)
                            }).ToList(); // must call .ToList here else the output will be wrapped in $type,$values

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        protected override void DeleteQuery(OrganizationUnit entity)
        {
            _orgUnitService.Delete(entity.Id);
        }
    }
}
