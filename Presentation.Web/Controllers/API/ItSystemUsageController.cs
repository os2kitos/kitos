using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Castle.Core.Internal;
using Core.ApplicationServices;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ItSystemUsageController : GenericContextAwareApiController<ItSystemUsage, ItSystemUsageDTO>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IItSystemUsageService _itSystemUsageService;
        private readonly IGenericRepository<ItSystemRole> _roleRepository;
        private readonly IGenericRepository<AttachedOption> _attachedOptionsRepository;


        public ItSystemUsageController(IGenericRepository<ItSystemUsage> repository,
            IGenericRepository<OrganizationUnit> orgUnitRepository,
            IGenericRepository<TaskRef> taskRepository,
            IItSystemUsageService itSystemUsageService,
            IGenericRepository<ItSystemRole> roleRepository,
            IGenericRepository<AttachedOption> attachedOptionsRepository,
            IAuthorizationContext authorizationContext)
            : base(repository, authorizationContext)
        {
            _orgUnitRepository = orgUnitRepository;
            _taskRepository = taskRepository;
            _itSystemUsageService = itSystemUsageService;
            _roleRepository = roleRepository;
            _attachedOptionsRepository = attachedOptionsRepository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItSystemUsageDTO>>))]
        public HttpResponseMessage GetSearchByOrganization(int organizationId, string q)
        {
            try
            {
                if (!AllowOrganizationReadAccess(organizationId))
                {
                    return Forbidden();
                }
                var usages = Repository.Get(
                    u =>
                        // filter by system usage name
                        u.ItSystem.Name.Contains(q) &&
                        // system usage is only within the context
                        u.OrganizationId == organizationId);

                return Ok(Map(usages));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItSystemUsageDTO>>))]
        public HttpResponseMessage GetByOrganization(int organizationId, [FromUri] PagingModel<ItSystemUsage> pagingModel, [FromUri] string q, bool? overview)
        {
            try
            {
                if (!AllowOrganizationReadAccess(organizationId))
                {
                    return Forbidden();
                }

                pagingModel.Where(
                    u =>
                        // system usage is only within the context
                        u.OrganizationId == organizationId
                    );

                if (!string.IsNullOrEmpty(q)) pagingModel.Where(usage => usage.ItSystem.Name.Contains(q));

                var usages = Page(Repository.AsQueryable(), pagingModel);

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

                if (!AllowRead(item))
                {
                    return Forbidden();
                }

                if (item == null)
                {
                    return NotFound();
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

        public HttpResponseMessage GetExcel(bool? csv, int organizationId)
        {
            try
            {
                var usages = Repository.Get(
                    u =>
                        // system usage is only within the context
                        u.OrganizationId == organizationId
                    );

                // mapping to DTOs for easy lazy loading of needed properties
                usages = usages.Where(AllowRead);
                var dtos = Map(usages);

                var roles = _roleRepository.Get();
                roles = roles.Where(AllowRead).ToList();

                var list = new List<dynamic>();
                var header = new ExpandoObject() as System.Collections.Generic.IDictionary<string, Object>;
                header.Add("Aktiv", "Aktiv");
                header.Add("IT System", "IT System");
                header.Add("OrgUnit", "Ansv. organisationsenhed");
                foreach (var role in roles)
                    header.Add(role.Name, role.Name);
                header.Add("AppType", "Applikationtype");
                header.Add("BusiType", "Forretningstype");
                header.Add("Anvender", "Anvender");
                header.Add("Udstiller", "Udstiller");
                header.Add("Overblik", "Overblik");
                list.Add(header);
                foreach (var usage in dtos)
                {
                    var obj = new ExpandoObject() as IDictionary<string, Object>;
                    obj.Add("Aktiv", usage.MainContractIsActive);
                    obj.Add("IT System", usage.ItSystem.Name);
                    obj.Add("OrgUnit", usage.ResponsibleOrgUnitName);
                    foreach (var role in roles)
                    {
                        var roleId = role.Id;
                        obj.Add(role.Name,
                                String.Join(",", usage.Rights.Where(x => x.RoleId == roleId).Select(x => x.User.FullName)));
                    }
                    obj.Add("AppType", usage.ItSystem.AppTypeOptionName);
                    obj.Add("BusiType", usage.ItSystem.BusinessTypeName);
                    obj.Add("Anvender", usage.ActiveInterfaceUseCount + "(" + usage.InterfaceUseCount + ")");
                    obj.Add("Udstiller", usage.InterfaceExhibitCount);
                    obj.Add("Overblik", usage.OverviewItSystemName);
                    list.Add(obj);
                }

                var s = list.ToCsv();
                var bytes = Encoding.Unicode.GetBytes(s);
                var stream = new MemoryStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);

                var result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileNameStar = "itsystemanvendelsesoversigt.csv", DispositionType = "ISO-8859-1" };
                return result;
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
                var usage = Repository.Get(u => u.ItSystemId == itSystemId && u.OrganizationId == organizationId).FirstOrDefault();

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
                var itsystemUsage = AutoMapper.Mapper.Map<ItSystemUsageDTO, ItSystemUsage>(dto);

                if (!AllowCreate<ItSystemUsage>(itsystemUsage))
                {
                    return Forbidden();
                }

                if (Repository.Get(usage => usage.ItSystemId == dto.ItSystemId
                                            && usage.OrganizationId == dto.OrganizationId).Any())
                {
                    return Conflict("Usage already exist");
                }

                var sysUsage = _itSystemUsageService.Add(itsystemUsage, KitosUser);
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
                var usage = Repository.Get(u => u.ItSystemId == itSystemId && u.OrganizationId == organizationId).FirstOrDefault();
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
