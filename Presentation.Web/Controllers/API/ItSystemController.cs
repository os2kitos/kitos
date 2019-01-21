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
using Core.DomainModel.Organization;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ItSystemController : GenericHierarchyApiController<ItSystem, ItSystemDTO>
    {
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IItSystemService _systemService;
        private readonly ReferenceService _referenceService;

        public ItSystemController(IGenericRepository<ItSystem> repository, IGenericRepository<TaskRef> taskRepository, IItSystemService systemService, ReferenceService referenceService)
            : base(repository)
        {
            _taskRepository = taskRepository;
            _systemService = systemService;
            _referenceService = referenceService;
        }

        // DELETE api/T
        public override HttpResponseMessage Delete(int id, int organizationId)
        {
            try
            {
                var item = Repository.GetByKey(id);

                // check if system has any usages, if it does it's may not be deleted
                if (item.Usages.Any())
                    return Conflict("Cannot delete a system in use!");

                // OS2KITOS-796: Handles cascading delete of references when deleting an IT System
                if (item.ExternalReferences.Any())
                {
                    var ids = item.ExternalReferences.ToList().Select(t => t.Id);
                    _referenceService.Delete(ids);
                }

                return base.Delete(id, organizationId);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        protected override void DeleteQuery(ItSystem entity)
        {
            _systemService.Delete(entity.Id);
        }

        public HttpResponseMessage GetPublic([FromUri] int organizationId, [FromUri] PagingModel<ItSystem> paging, [FromUri] string q)
        {
            try
            {
                //Get all systems which the user has access to
                paging.Where(
                    s =>
                        // it's public everyone can see it
                        s.AccessModifier == AccessModifier.Public ||
                        // It's in the right context
                        s.OrganizationId == organizationId &&
                        // global admin sees all within the context
                        (KitosUser.IsGlobalAdmin ||
                        // object owner sees his own objects within the given context
                        s.ObjectOwnerId == KitosUser.Id ||
                        // everyone in the same organization can see normal objects
                        s.AccessModifier == AccessModifier.Local)
                    // it systems doesn't have roles so private doesn't make sense
                    // only object owners will be albe to see private objects
                    );

                if (!string.IsNullOrEmpty(q)) paging.Where(sys => sys.Name.Contains(q));

                var query = Page(Repository.AsQueryable(), paging);

                return Ok(Map(query));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetExcel([FromUri] bool? csv, [FromUri] int organizationId)
        {
            try
            {
                var systems =
                    Repository.AsQueryable()
                        .Where(s =>
                            // global admin sees all
                            (KitosUser.IsGlobalAdmin ||
                            // object owner sees his own objects
                            s.ObjectOwnerId == KitosUser.Id ||
                            // it's public everyone can see it
                            s.AccessModifier == AccessModifier.Public ||
                            // everyone in the same organization can see normal objects
                            s.AccessModifier == AccessModifier.Local &&
                            s.OrganizationId == organizationId
                        // it systems doesn't have roles so private doesn't make sense
                        // only object owners will be albe to see private objects
                        ));

                //if (!string.IsNullOrEmpty(q)) paging.Where(sys => sys.Name.Contains(q));

                //var query = Page(systems, paging);

                var dtos = Map(systems);

                var list = new List<dynamic>();
                var header = new ExpandoObject() as IDictionary<string, Object>;
                header.Add("It System", "It System");
                header.Add("Public", "(P)");
                header.Add("AppTypeOption", "Applikationstype");
                header.Add("BusiType", "Forretningstype");
                header.Add("KLEID", "KLE ID");
                header.Add("KLENavn", "KLE Navn");
                header.Add("Rettighedshaver", "Rettighedshaver");
                header.Add("Oprettet", "Oprettet af");
                list.Add(header);
                foreach (var system in dtos)
                {
                    var obj = new ExpandoObject() as IDictionary<string, Object>;
                    obj.Add("It System", system.Name);
                    obj.Add("Public", system.AccessModifier == AccessModifier.Public ? "(P)" : "");
                    obj.Add("AppType", system.AppTypeOptionName);
                    obj.Add("BusiType", system.BusinessTypeName);
                    obj.Add("KLEID", String.Join(",", system.TaskRefs.Select(x => x.TaskKey)));
                    obj.Add("KLENavn", String.Join(",", system.TaskRefs.Select(x => x.Description)));
                    obj.Add("Rettighedshaver", system.BelongsToName);
                    obj.Add("Oprettet", system.ObjectOwnerFullName);
                    list.Add(obj);
                }
                var csvList = list.ToCsv();
                var bytes = Encoding.Unicode.GetBytes(csvList);
                var stream = new MemoryStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);

                var result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileNameStar = "itsystemkatalog.csv", DispositionType = "ISO-8859-1" };
                return result;
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetInterfacesSearch(string q, int orgId, int excludeId)
        {
            try
            {
                var systems = Repository.Get(
                    s =>
                        // filter by name
                        s.Name.Contains(q) &&
                        // exclude system with id
                        s.Id != excludeId &&
                        // global admin sees all
                        (KitosUser.IsGlobalAdmin ||
                         // object owner sees his own objects
                         s.ObjectOwnerId == KitosUser.Id ||
                         // it's public everyone can see it
                         s.AccessModifier == AccessModifier.Public ||
                         // everyone in the same organization can see normal objects
                         s.AccessModifier == AccessModifier.Local &&
                         s.OrganizationId == orgId)
                    // it systems doesn't have roles so private doesn't make sense
                    // only object owners will be albe to see private objects
                    );
                var dtos = Map(systems);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetInterfacesSearch(string q, int orgId, bool? interfaces)
        {
            try
            {
                var systems = _systemService.GetInterfaces(orgId, q, KitosUser);
                var dtos = Map(systems);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetNonInterfacesSearch(string q, int orgId, bool? nonInterfaces)
        {
            try
            {
                var systems = _systemService.GetNonInterfaces(orgId, q, KitosUser);
                var dtos = Map(systems);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetHierarchy(int id, [FromUri] bool hierarchy)
        {
            try
            {
                var systems = _systemService.GetHierarchy(id);
                return Ok(Map(systems));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public override HttpResponseMessage Post(ItSystemDTO dto)
        {
            try
            {
                // only global admin can set access mod to public
                if (dto.AccessModifier == AccessModifier.Public && !KitosUser.IsGlobalAdmin)
                {
                    return Unauthorized();
                }

                if (!IsAvailable(dto.Name, dto.OrganizationId))
                    return Conflict("Name is already taken!");

                var item = Map(dto);

                item.ObjectOwner = KitosUser;
                item.LastChangedByUser = KitosUser;
                item.Uuid = Guid.NewGuid();

                foreach (var id in dto.TaskRefIds)
                {
                    var task = _taskRepository.GetByKey(id);
                    item.TaskRefs.Add(task);
                }

                var savedItem = PostQuery(item);

                return Created(Map(savedItem), new Uri(Request.RequestUri + "/" + savedItem.Id));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetItSystemsUsedByOrg([FromUri] int orgId)
        {
            try
            {
                var systems = Repository.Get(x => x.OrganizationId == orgId || x.Usages.Any(y => y.OrganizationId == orgId));

                return systems == null ? NotFound() : Ok(Map(systems));
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
                var system = Repository.GetByKey(id);
                if (system == null) return NotFound();
                if (!HasWriteAccess(system, organizationId)) return Unauthorized();

                List<TaskRef> tasks;
                if (taskId.HasValue)
                {
                    // get child leaves of taskId that havn't got a usage in the system
                    tasks = _taskRepository.Get(
                        x =>
                            (x.Id == taskId || x.ParentId == taskId || x.Parent.ParentId == taskId) && !x.Children.Any() &&
                            x.AccessModifier == AccessModifier.Public &&
                            x.ItSystems.All(y => y.Id != id)).ToList();
                }
                else
                {
                    // no taskId was specified so get everything
                    tasks = _taskRepository.Get(
                        x =>
                            !x.Children.Any() &&
                            x.AccessModifier == AccessModifier.Public &&
                            x.ItSystems.All(y => y.Id != id)).ToList();
                }

                if (!tasks.Any())
                    return NotFound();

                foreach (var task in tasks)
                {
                    system.TaskRefs.Add(task);
                }
                system.LastChanged = DateTime.UtcNow;
                system.LastChangedByUser = KitosUser;
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
                var system = Repository.GetByKey(id);
                if (system == null) return NotFound();
                if (!HasWriteAccess(system, organizationId)) return Unauthorized();

                List<TaskRef> tasks;
                if (taskId.HasValue)
                {
                    tasks =
                        system.TaskRefs.Where(
                            x =>
                                (x.Id == taskId || x.ParentId == taskId || x.Parent.ParentId == taskId) &&
                                !x.Children.Any())
                            .ToList();
                }
                else
                {
                    // no taskId was specified so get everything
                    tasks = system.TaskRefs.ToList();
                }

                if (!tasks.Any())
                    return NotFound();

                foreach (var task in tasks)
                {
                    system.TaskRefs.Remove(task);
                }
                system.LastChanged = DateTime.UtcNow;
                system.LastChangedByUser = KitosUser;
                Repository.Save();
                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        /// <summary>
        /// Returns a list of task ref and whether or not they are currently associated with a given system
        /// </summary>
        /// <param name="id">ID of the system</param>
        /// <param name="tasks">Routing qualifer</param>
        /// <param name="onlySelected">If true, only return those task ref that are associated with the system. If false, return all task ref.</param>
        /// <param name="taskGroup">Optional filtering on task group</param>
        /// <param name="pagingModel">Paging model</param>
        /// <returns>List of TaskRefSelectedDTO</returns>
        public HttpResponseMessage GetTasks(int id, bool? tasks, bool onlySelected, int? taskGroup, [FromUri] PagingModel<TaskRef> pagingModel)
        {
            try
            {
                var system = Repository.GetByKey(id);

                IQueryable<TaskRef> taskQuery;
                if (onlySelected)
                {
                    taskQuery = Repository.AsQueryable().Where(p => p.Id == id).SelectMany(p => p.TaskRefs);
                }
                else
                {
                    taskQuery = _taskRepository.AsQueryable();
                }

                // if a task group is given, only find the tasks in that group
                if (taskGroup.HasValue)
                    pagingModel.Where(taskRef => (taskRef.ParentId.Value == taskGroup.Value ||
                                                  taskRef.Parent.ParentId.Value == taskGroup.Value) &&
                                                 !taskRef.Children.Any() &&
                                                 taskRef.AccessModifier == AccessModifier.Public); // TODO add support for normal
                else
                    pagingModel.Where(taskRef => taskRef.Children.Count == 0);

                var theTasks = Page(taskQuery, pagingModel).ToList();

                var dtos = theTasks.Select(task => new TaskRefSelectedDTO()
                {
                    TaskRef = Map<TaskRef, TaskRefDTO>(task),
                    IsSelected = onlySelected || system.TaskRefs.Any(t => t.Id == task.Id)
                }).ToList(); // must call .ToList here else the output will be wrapped in $type,$values

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        {
            // try get AccessModifier value
            JToken accessModToken;
            obj.TryGetValue("accessModifier", out accessModToken);
            // only global admin can set access mod to public
            if (accessModToken != null && accessModToken.ToObject<AccessModifier>() == AccessModifier.Public && !KitosUser.IsGlobalAdmin)
            {
                return Unauthorized();
            }

            // try get name value
            JToken nameToken;
            obj.TryGetValue("name", out nameToken);
            if (nameToken != null)
            {
                string name = nameToken.Value<string>();
                var system = Repository.Get(x => x.Name == name && x.OrganizationId == organizationId && x.Id != id);
                if (system.Any())
                    return Conflict("Name is already taken!");
            }

            return base.Patch(id, organizationId, obj);
        }

        public HttpResponseMessage GetNameAvailable(string checkname, int orgId)
        {
            try
            {
                return IsAvailable(checkname, orgId) ? Ok() : Conflict("Name is already taken!");
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        private bool IsAvailable(string name, int orgId)
        {
            var system = Repository.Get(x => x.Name == name && x.OrganizationId == orgId);
            return !system.Any();
        }

        protected override bool HasWriteAccess(ItSystem obj, User user, int organizationId)
        {
            if (obj.IsInContext(organizationId) && user.OrganizationRights.Any(x => x.OrganizationId == organizationId && (x.Role == OrganizationRole.LocalAdmin || x.Role == OrganizationRole.SystemModuleAdmin)))
            {
                return true;
            }
            if (user.IsLocalAdmin && obj.ObjectOwnerId == user.Id && user.DefaultOrganizationId == organizationId)
            {
                return true;
            }
            return HasWriteAccess();
        }

        protected bool HasWriteAccess()
        {
            return KitosUser.IsGlobalAdmin;
        }
    }
}
