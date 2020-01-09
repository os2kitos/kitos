using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Castle.Core.Internal;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [MigratedToNewAuthorizationContext]
    public class ItSystemUsageController : GenericContextAwareApiController<ItSystemUsage, ItSystemUsageDTO>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IItSystemUsageService _itSystemUsageService;
        private readonly IGenericRepository<AttachedOption> _attachedOptionsRepository;


        public ItSystemUsageController(IGenericRepository<ItSystemUsage> repository,
            IGenericRepository<OrganizationUnit> orgUnitRepository,
            IGenericRepository<TaskRef> taskRepository,
            IItSystemUsageService itSystemUsageService,
            IGenericRepository<AttachedOption> attachedOptionsRepository,
            IAuthorizationContext authorizationContext)
            : base(repository, authorizationContext)
        {
            _orgUnitRepository = orgUnitRepository;
            _taskRepository = taskRepository;
            _itSystemUsageService = itSystemUsageService;
            _attachedOptionsRepository = attachedOptionsRepository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItSystemUsageDTO>>))]
        public HttpResponseMessage GetSearchByOrganization(int organizationId, string q)
        {
            try
            {
                //Local objects - must have full access to view
                if (GetOrganizationReadAccessLevel(organizationId) != OrganizationDataReadAccessLevel.All)
                {
                    return Forbidden();
                }

                var usages = Repository
                    .AsQueryable()
                    .ByOrganizationId(organizationId);

                if (!string.IsNullOrWhiteSpace(q))
                {
                    usages = usages.Where(usage => usage.ItSystem.Name.Contains(q));
                }

                return Ok(Map(usages));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<ItSystemUsageDTO>))]
        public override HttpResponseMessage GetSingle(int id)
        {

            try
            {
                var item = Repository.GetByKey(id);

                if (item == null)
                {
                    return NotFound();
                }

                if (!AllowRead(item))
                {
                    return Forbidden();
                }

                var dto = Map(item);

                if (item.OrganizationId != KitosUser.DefaultOrganizationId)
                {
                    dto.Note = "";
                }

                return Ok(dto);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<ItSystemUsageDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetByItSystemAndOrganization(int itSystemId, int organizationId)
        {
            try
            {
                var usage = _itSystemUsageService.GetByOrganizationAndSystemId(organizationId, itSystemId);

                if (usage == null)
                {
                    return NotFound();
                }

                if (!AllowRead(usage))
                {
                    return Forbidden();
                }

                return Ok(Map(usage));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public override HttpResponseMessage Post(ItSystemUsageDTO dto)
        {
            try
            {
                var usage = AutoMapper.Mapper.Map<ItSystemUsageDTO, ItSystemUsage>(dto);

                if (!AllowCreate<ItSystemUsage>(usage))
                {
                    return Forbidden();
                }

                if (Repository.Get(usage => usage.ItSystemId == dto.ItSystemId
                                            && usage.OrganizationId == dto.OrganizationId).Any())
                {
                    return Conflict("Usage already exist");
                }

                var sysUsage = _itSystemUsageService.Add(usage, KitosUser);
                sysUsage.DataLevel = dto.DataLevel;

                //copy attached options from system to systemusage
                var attachedOptions = _attachedOptionsRepository.AsQueryable().Where(a => a.ObjectId == sysUsage.ItSystemId && a.ObjectType == EntityType.ITSYSTEM);
                foreach (var option in attachedOptions)
                {
                    option.ObjectId = sysUsage.Id;
                    option.ObjectType = EntityType.ITSYSTEMUSAGE;
                    _attachedOptionsRepository.Insert(option);
                }
                _attachedOptionsRepository.Save();


                return Created(Map(sysUsage), new Uri(Request.RequestUri + "?itSystemId=" + dto.ItSystemId + "&organizationId" + dto.OrganizationId));

            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage DeleteByItSystemId(int itSystemId, int organizationId)
        {
            try
            {
                var usage = _itSystemUsageService.GetByOrganizationAndSystemId(organizationId, itSystemId);

                if (usage == null)
                {
                    return NotFound();
                }

                //This will make sure we check for permissions and such...
                return base.Delete(usage.Id, organizationId);

            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage PostOrganizationUnitsUsingThisSystem(int id, [FromUri] int organizationUnit, int organizationId)
        {
            try
            {
                var usage = Repository.GetByKey(id);
                if (usage == null)
                {
                    return NotFound();
                }
                if (!AllowModify(usage))
                {
                    return Forbidden();
                }

                var orgUnit = _orgUnitRepository.GetByKey(organizationUnit);
                if (orgUnit == null)
                {
                    return NotFound();
                }

                usage.UsedBy.Add(new ItSystemUsageOrgUnitUsage { ItSystemUsageId = id, OrganizationUnitId = organizationUnit });

                usage.LastChanged = DateTime.UtcNow;
                usage.LastChangedByUser = KitosUser;

                Repository.Save();

                return Created(Map<OrganizationUnit, OrgUnitDTO>(orgUnit));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage DeleteOrganizationUnitsUsingThisSystem(int id, [FromUri] int organizationUnit, int organizationId)
        {
            try
            {
                var usage = Repository.GetByKey(id);
                if (usage == null)
                {
                    return NotFound();
                }

                if (!AllowModify(usage))
                {
                    return Forbidden();
                }

                var orgUnit = _orgUnitRepository.GetByKey(organizationUnit);
                if (orgUnit == null)
                {
                    return NotFound();
                }

                var entity = usage.UsedBy.SingleOrDefault(x => x.ItSystemUsageId == id && x.OrganizationUnitId == organizationUnit);
                if (entity == null)
                {
                    return NotFound();
                }

                usage.UsedBy.Remove(entity);

                usage.LastChanged = DateTime.UtcNow;
                usage.LastChangedByUser = KitosUser;

                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage PostTasksUsedByThisSystem(int id, int organizationId, [FromUri] int? taskId)
        {
            try
            {
                var usage = Repository.GetByKey(id);
                if (usage == null) return NotFound();
                if (!AllowModify(usage))
                {
                    return Forbidden();
                }

                List<TaskRef> tasks;
                if (taskId.HasValue)
                {
                    // get child leaves of taskId that havn't got a usage in the system
                    tasks = _taskRepository.Get(
                        x =>
                            (x.Id == taskId || x.ParentId == taskId || x.Parent.ParentId == taskId) && !x.Children.Any() &&
                            x.AccessModifier == AccessModifier.Public &&
                            x.ItSystemUsages.All(y => y.Id != id)).ToList();
                }
                else
                {
                    // no taskId was specified so get everything
                    tasks = _taskRepository.Get(
                        x =>
                            !x.Children.Any() &&
                            x.AccessModifier == AccessModifier.Public &&
                            x.ItSystemUsages.All(y => y.Id != id)).ToList();
                }

                if (!tasks.Any())
                    return NotFound();

                foreach (var task in tasks)
                {
                    if (!usage.ItSystem.TaskRefs.Any(t => t.Id == task.Id))
                    {
                        usage.TaskRefs.Add(task);
                    }
                    // If the task is contained among the inherited tasks remove it from the list of opt out's
                    if (usage.TaskRefsOptOut.Any(u => u.Id == task.Id))
                    {
                        usage.TaskRefsOptOut.Remove(task);
                    }
                }
                usage.LastChanged = DateTime.UtcNow;
                usage.LastChangedByUser = KitosUser;
                Repository.Save();
                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage DeleteTasksUsedByThisSystem(int id, int organizationId, [FromUri] int? taskId)
        {
            try
            {
                var usage = Repository.GetByKey(id);
                if (usage == null)
                {
                    return NotFound();
                }

                if (!AllowModify(usage))
                {
                    return Forbidden();
                }
                var optOut = false;

                List<TaskRef> tasks;
                if (taskId.HasValue)
                {
                    tasks = usage.TaskRefs.Where(x => (x.Id == taskId || x.ParentId == taskId || x.Parent.ParentId == taskId) && !x.Children.Any()).ToList();

                    if (tasks.IsNullOrEmpty())
                    {
                        optOut = true;
                        tasks = usage.ItSystem.TaskRefs.Where(x => (x.Id == taskId || x.ParentId == taskId || x.Parent.ParentId == taskId) && !x.Children.Any()).ToList();
                    }
                }
                else
                {
                    // no taskId was specified so get everything
                    tasks = usage.TaskRefs.ToList();

                    if (tasks.IsNullOrEmpty())
                    {
                        tasks = usage.ItSystem.TaskRefs.ToList();
                    }
                }

                if (!tasks.Any())
                    return NotFound();

                foreach (var task in tasks)
                {
                    if (!optOut)
                    {
                        usage.TaskRefs.Remove(task);
                    }
                    // If the task is contained among the inherited tasks add it to the list of opt out's
                    if (usage.ItSystem.TaskRefs.Any(u => u.Id == task.Id))
                    {
                        usage.TaskRefsOptOut.Add(task);
                    }
                }
                usage.LastChanged = DateTime.UtcNow;
                usage.LastChangedByUser = KitosUser;
                Repository.Save();
                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        /// <summary>
        /// Returns a list of task ref and whether or not they are currently associated with a given IT system usage
        /// </summary>
        /// <param name="id">ID of the IT system usage</param>
        /// <param name="tasks">Routing qualifer</param>
        /// <param name="onlySelected">If true, only return those task ref that are associated with the I system usage. If false, return all task ref.</param>
        /// <param name="taskGroup">Optional filtering on task group</param>
        /// <param name="pagingModel">Paging model</param>
        /// <returns>List of TaskRefSelectedDTO</returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<List<TaskRefSelectedDTO>>))]
        public HttpResponseMessage GetTasks(int id, bool? tasks, bool onlySelected, int? taskGroup, [FromUri] PagingModel<TaskRef> pagingModel)
        {
            try
            {
                var usage = Repository.GetByKey(id);

                if (!AllowRead(usage))
                {
                    return Forbidden();
                }

                IQueryable<TaskRef> taskQuery;
                if (onlySelected)
                {
                    var usedTasks = Repository.AsQueryable().Where(p => p.Id == id).SelectMany(p => p.TaskRefs);
                    var inheritedTasks = Repository.AsQueryable().Where(p => p.Id == id).Select(p => p.ItSystem).SelectMany(s => s.TaskRefs);
                    var optOuts = Repository.AsQueryable().Where(p => p.Id == id).SelectMany(s => s.TaskRefsOptOut);
                    taskQuery = usedTasks.Union(inheritedTasks);
                    taskQuery = taskQuery.Except(optOuts);
                }
                else
                {
                    taskQuery = _taskRepository.AsQueryable();
                }

                //if a task group is given, only find the tasks in that group
                if (taskGroup.HasValue)
                    pagingModel.Where(taskRef => (taskRef.ParentId.Value == taskGroup.Value ||
                                                  taskRef.Parent.ParentId.Value == taskGroup.Value) &&
                                                 !taskRef.Children.Any() &&
                                                 taskRef.AccessModifier == AccessModifier.Public); // TODO add support for normal
                else
                    pagingModel.Where(taskRef => taskRef.Children.Count == 0);

                pagingModel.WithPostProcessingFilter(AllowRead);
                var theTasks = Page(taskQuery, pagingModel).ToList();

                var dtos = theTasks.Select(task => new TaskRefSelectedDTO()
                {
                    TaskRef = Map<TaskRef, TaskRefDTO>(task),
                    // a task is selected if it is contained in either usage.TaskRefs or usage.ItSystem.TaskRefs but not in usage.TaskRefsOptOut
                    IsSelected = (onlySelected || usage.TaskRefs.Any(t => t.Id == task.Id) || usage.ItSystem.TaskRefs.Any(t => t.Id == task.Id)) && !usage.TaskRefsOptOut.Any(t => t.Id == task.Id),
                    Inherited = usage.ItSystem.TaskRefs.Any(t => t.Id == task.Id)

                }).ToList(); // must call .ToList here else the output will be wrapped in $type,$values

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        protected override void DeleteQuery(ItSystemUsage entity)
        {
            _itSystemUsageService.Delete(entity.Id);
        }
    }
}
