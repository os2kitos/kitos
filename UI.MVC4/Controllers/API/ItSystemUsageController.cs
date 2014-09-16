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
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemUsageController : GenericApiController<ItSystemUsage, ItSystemUsageDTO> 
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IItSystemUsageService _itSystemUsageService;
        private readonly IGenericRepository<ItSystemRole> _roleRepository;

        public ItSystemUsageController(IGenericRepository<ItSystemUsage> repository,
            IGenericRepository<OrganizationUnit> orgUnitRepository, 
            IGenericRepository<TaskRef> taskRepository, 
            IItSystemUsageService itSystemUsageService, 
            IGenericRepository<ItSystemRole> roleRepository) 
            : base(repository)
        {
            _orgUnitRepository = orgUnitRepository;
            _taskRepository = taskRepository;
            _itSystemUsageService = itSystemUsageService;
            _roleRepository = roleRepository;
        }

        public HttpResponseMessage GetSearchByOrganization(int organizationId, string q)
        {
            try
            {
                var usages = Repository.Get(
                    u =>
                        // filter by system usage name
                        u.ItSystem.Name.Contains(q) &&
                        // global admin sees all within the context 
                        KitosUser.IsGlobalAdmin && u.OrganizationId == organizationId ||
                        // object owner sees his own objects     
                        u.ObjectOwnerId == KitosUser.Id ||
                        // it's public everyone can see it
                        u.ItSystem.AccessModifier == AccessModifier.Public ||
                        // everyone in the same organization can see normal objects
                        u.ItSystem.AccessModifier == AccessModifier.Normal &&
                        u.ItSystem.OrganizationId == organizationId ||
                        // only users with a role on the object can see private objects
                        u.ItSystem.AccessModifier == AccessModifier.Private &&
                        u.Rights.Any(x => x.UserId == KitosUser.Id)
                    );

                return Ok(Map(usages));
            }
            catch (Exception e)
            {
                return Error(e);
            } 
        }

        public HttpResponseMessage GetByOrganization(int organizationId, [FromUri] PagingModel<ItSystemUsage> pagingModel, [FromUri] string q, bool? overview)
        {
            try
            {
                pagingModel.Where(
                    u =>
                        // global admin sees all within the context 
                        KitosUser.IsGlobalAdmin && u.OrganizationId == organizationId ||
                        // object owner sees his own objects     
                        u.ObjectOwnerId == KitosUser.Id ||
                        // it's public everyone can see it
                        u.ItSystem.AccessModifier == AccessModifier.Public ||
                        // everyone in the same organization can see normal objects
                        u.ItSystem.AccessModifier == AccessModifier.Normal &&
                        u.ItSystem.OrganizationId == organizationId ||
                        // only users with a role on the object can see private objects
                        u.ItSystem.AccessModifier == AccessModifier.Private &&
                        u.Rights.Any(x => x.UserId == KitosUser.Id)
                    );

                if (!string.IsNullOrEmpty(q)) pagingModel.Where(usage => usage.ItSystem.Name.Contains(q));

                var usages = Page(Repository.AsQueryable(), pagingModel);
                
                return Ok(Map(usages));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetExcel(int organizationId, [FromUri] PagingModel<ItSystemUsage> pagingModel, [FromUri] string q, bool? csv)
        {
            try
            {
                pagingModel.Where(
                    u =>
                        // global admin sees all within the context 
                        KitosUser.IsGlobalAdmin && u.OrganizationId == organizationId ||
                        // object owner sees his own objects     
                        u.ObjectOwnerId == KitosUser.Id ||
                        // it's public everyone can see it
                        u.ItSystem.AccessModifier == AccessModifier.Public ||
                        // everyone in the same organization can see normal objects
                        u.ItSystem.AccessModifier == AccessModifier.Normal &&
                        u.ItSystem.OrganizationId == organizationId ||
                        // only users with a role on the object can see private objects
                        u.ItSystem.AccessModifier == AccessModifier.Private &&
                        u.Rights.Any(x => x.UserId == KitosUser.Id)
                    );

                if (!string.IsNullOrEmpty(q)) pagingModel.Where(usage => usage.ItSystem.Name.Contains(q));

                var usages = Page(Repository.AsQueryable(), pagingModel);
                // mapping to DTOs for easy lazy loading of needed properties
                var dtos = Map(usages);

                var roles = _roleRepository.Get().ToList();

                var list = new List<dynamic>();
                var header = new ExpandoObject() as IDictionary<string, Object>;
                header.Add("Aktiv", "Aktiv");
                header.Add("IT System", "IT System");
                header.Add("OrgUnit", "Ansv. organisationsenhed");
                foreach (var role in roles)
                    header.Add(role.Name, role.Name);
                header.Add("Gid", "Globalt SystemID");
                header.Add("Lid", "Lokalt SystemID");
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
                                String.Join(",", usage.Rights.Where(x => x.RoleId == roleId).Select(x => x.User.Name)));
                    }
                    obj.Add("Gid", usage.ItSystem.ItSystemId);
                    obj.Add("Lid", usage.LocalSystemId);
                    obj.Add("AppType", usage.ItSystem.AppTypeName);
                    obj.Add("BusiType", usage.ItSystem.BusinessTypeName);
                    //obj.Add("Anvender", usage.ActiveInterfaceUsages + "(" + usage.ItSystem.CanUseInterfaceIds.Count() + ")"); TODO
                    //obj.Add("Udstiller", usage.ItSystem.ExposedBy != null ? usage.ItSystem.ExposedBy.Name : "" + " " + usage.ItSystem.ExposedInterfaceIds.Count()); TODO
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
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "itsystemanvendelsesoversigt.csv" };
                return result;
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetByItSystemAndOrganization(int itSystemId, int organizationId)
        {
            try
            {
                var usage = Repository.Get(u => u.ItSystemId == itSystemId && u.OrganizationId == organizationId).FirstOrDefault();

                return usage == null ? NotFound() : Ok(Map(usage));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public override HttpResponseMessage Post(ItSystemUsageDTO dto)
        {
            try
            {
                if (Repository.Get(usage => usage.ItSystemId == dto.ItSystemId
                                            && usage.OrganizationId == dto.OrganizationId).Any())
                    return Conflict("Usage already exist");

                var sysUsage = _itSystemUsageService.Add(dto.ItSystemId, dto.OrganizationId, KitosUser);

                return Created(Map(sysUsage), new Uri(Request.RequestUri + "?itSystemId=" + dto.ItSystemId + "&organizationId" + dto.OrganizationId));

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Delete(int itSystemId, int organizationId)
        {
            try
            {
                var usage = Repository.Get(u => u.ItSystemId == itSystemId && u.OrganizationId == organizationId).FirstOrDefault();
                if (usage == null) return NotFound();

                //This will make sure we check for permissions and such...
                return Delete(usage.Id);

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage PostOrganizationUnitsUsingThisSystem(int id, [FromUri] int organizationUnit)
        {
            try
            {
                var usage = Repository.GetByKey(id);
                if (usage == null) return NotFound();
                if (!HasWriteAccess(usage)) return Unauthorized();

                var orgUnit = _orgUnitRepository.GetByKey(organizationUnit);
                if (orgUnit == null) return NotFound();


                usage.UsedBy.Add(new ItSystemUsageOrgUnitUsage { ItSystemUsageId = id, OrganizationUnitId = organizationUnit });

                usage.LastChanged = DateTime.Now;
                usage.LastChangedByUser = KitosUser;

                Repository.Save();

                return Created(Map<OrganizationUnit, OrgUnitDTO>(orgUnit));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage DeleteOrganizationUnitsUsingThisSystem(int id, [FromUri] int organizationUnit)
        {
            try
            {
                var usage = Repository.GetByKey(id);
                if (usage == null) return NotFound();

                if (!HasWriteAccess(usage)) return Unauthorized();

                var orgUnit = _orgUnitRepository.GetByKey(organizationUnit);
                if (orgUnit == null) return NotFound();

                var entity = usage.UsedBy.SingleOrDefault(x => x.ItSystemUsageId == id && x.OrganizationUnitId == organizationUnit);
                if (entity == null) return NotFound();

                usage.UsedBy.Remove(entity);

                usage.LastChanged = DateTime.Now;
                usage.LastChangedByUser = KitosUser;

                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage PostTasksUsedByThisSystem(int id, [FromUri] int taskId)
        {
            try
            {
                var usage = Repository.GetByKey(id);
                if (usage == null) return NotFound();
                if (!HasWriteAccess(usage)) return Unauthorized();

                var task = _taskRepository.GetByKey(taskId);
                if (task == null) return NotFound();

                usage.TaskRefs.Add(task);

                usage.LastChanged = DateTime.Now;
                usage.LastChangedByUser = KitosUser;

                Repository.Save();

                return Created(Map<TaskRef, TaskRefDTO>(task));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage DeleteTasksUsedByThisSystem(int id, [FromUri] int taskId)
        {
            try
            {
                var usage = Repository.GetByKey(id);
                if (usage == null) return NotFound();
                if (!HasWriteAccess(usage)) return Unauthorized();

                var task = _taskRepository.GetByKey(taskId);
                if (task == null) return NotFound();

                usage.TaskRefs.Remove(task);

                usage.LastChanged = DateTime.Now;
                usage.LastChangedByUser = KitosUser;

                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
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
        public HttpResponseMessage GetTasks(int id, bool? tasks, bool onlySelected, int? taskGroup, [FromUri] PagingModel<TaskRef> pagingModel)
        {
            try
            {
                var usage = Repository.GetByKey(id);

                IQueryable<TaskRef> taskQuery;
                if (onlySelected)
                {
                    var taskQuery1 = Repository.AsQueryable().Where(p => p.Id == id).SelectMany(p => p.TaskRefs);
                    var taskQuery2 =
                        Repository.AsQueryable()
                                  .Where(p => p.Id == id)
                                  .Select(p => p.ItSystem)
                                  .SelectMany(s => s.TaskRefs);
                    
                    taskQuery = taskQuery1.Union(taskQuery2);
                }
                else
                {
                    taskQuery = _taskRepository.AsQueryable();
                }

                //if a task group is given, only find the tasks in that group
                if (taskGroup.HasValue) pagingModel.Where(taskRef => taskRef.ParentId.Value == taskGroup.Value);
                else pagingModel.Where(taskRef => taskRef.Children.Count == 0);

                var theTasks = Page(taskQuery, pagingModel).ToList();

                var dtos = theTasks.Select(task => new TaskRefSelectedDTO()
                {
                    TaskRef = Map<TaskRef, TaskRefDTO>(task),
                    IsSelected = onlySelected || usage.TaskRefs.Any(t => t.Id == task.Id),
                    IsLocked = usage.ItSystem.TaskRefs.Any(t => t.Id == task.Id)
                }).ToList(); // must call .ToList here else the output will be wrapped in $type,$values

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
    }
}
