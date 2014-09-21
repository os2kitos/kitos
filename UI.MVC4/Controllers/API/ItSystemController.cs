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
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemController : GenericHierarchyApiController<ItSystem, ItSystemDTO>
    {
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IItSystemService _systemService;

        public ItSystemController(IGenericRepository<ItSystem> repository, IGenericRepository<TaskRef> taskRepository, IItSystemService systemService) 
            : base(repository)
        {
            _taskRepository = taskRepository;
            _systemService = systemService;
        }

        public HttpResponseMessage GetPublic([FromUri] int organizationId, [FromUri] PagingModel<ItSystem> paging, [FromUri] string q)
        {
            try
            {
                //Get all systems which the user has access to
                paging.Where(
                    s =>
                        // global admin sees all within the context 
                        KitosUser.IsGlobalAdmin && s.OrganizationId == organizationId ||
                        // object owner sees his own objects     
                        s.ObjectOwnerId == KitosUser.Id ||
                        // it's public everyone can see it
                        s.AccessModifier == AccessModifier.Public ||
                        // everyone in the same organization can see normal objects
                        s.AccessModifier == AccessModifier.Normal &&
                        s.OrganizationId == organizationId
                        // it systems doesn't have roles so private doesn't make sense
                        // only object owners will be albe to see private objects
                    );

                if (!string.IsNullOrEmpty(q)) paging.Where(sys => sys.Name.Contains(q));

                var query = Page(Repository.AsQueryable(), paging);

                return Ok(Map(query));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetExcel([FromUri] bool? csv, [FromUri] int organizationId, [FromUri] PagingModel<ItSystem> paging, [FromUri] string q)
        {
            try
            {
                var systems =
                    Repository.AsQueryable()
                        .Where(s =>
                            // global admin sees all within the context 
                            KitosUser.IsGlobalAdmin && s.OrganizationId == organizationId ||
                            // object owner sees his own objects     
                            s.ObjectOwnerId == KitosUser.Id ||
                            // it's public everyone can see it
                            s.AccessModifier == AccessModifier.Public ||
                            // everyone in the same organization can see normal objects
                            s.AccessModifier == AccessModifier.Normal &&
                            s.OrganizationId == organizationId
                            // it systems doesn't have roles so private doesn't make sense
                            // only object owners will be albe to see private objects
                        );

                if (!string.IsNullOrEmpty(q)) paging.Where(sys => sys.Name.Contains(q));

                var query = Page(systems, paging);

                var dtos = Map(query);
                
                var list = new List<dynamic>();
                var header = new ExpandoObject() as IDictionary<string, Object>;
                header.Add("It System", "It System");
                header.Add("ID", "Globalt SystemID");
                header.Add("AppType", "Applikationstype");
                header.Add("BusiType", "Forretningstype");
                header.Add("KLEID", "KLE ID");
                header.Add("KLENavn", "KLE Navn");
                header.Add("Tilhorer", "Tilhører");
                header.Add("Oprettet", "Oprettet af");
                list.Add(header);
                foreach (var system in dtos)
                {
                    var obj = new ExpandoObject() as IDictionary<string, Object>;
                    obj.Add("It Kontrakt", system.Name);
                    obj.Add("ID", system.ItSystemId);
                    obj.Add("AppType", system.AppType == 0 ? "Fagsystem" : "Fælleskommunal");
                    obj.Add("BusiType", system.BusinessTypeName);
                    obj.Add("KLEID", String.Join(",", system.TaskRefs.Select(x => x.TaskKey)));
                    obj.Add("KLENavn", String.Join(",", system.TaskRefs.Select(x => x.Description)));
                    obj.Add("Tilhorer", system.BelongsToName);
                    obj.Add("Oprettet", system.ObjectOwnerName);
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
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "itsystemanvendelsesoversigt.csv" };
                return result;
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        ///// <summary>
        ///// Returns the interfaces that a given system exposes
        ///// </summary>
        ///// <param name="itSystemId">The id of the exposing system</param>
        ///// <param name="getExposedInterfaces">flag</param>
        ///// <returns>List of interfaces</returns>
        //public HttpResponseMessage GetExposedInterfaces(int itSystemId, bool? getExposedInterfaces)
        //{
        //    try
        //    {
        //        var interfaces = Repository.Get(system => system.ExposedById == itSystemId);
        //        var dtos = Map(interfaces);
        //        return Ok(dtos);
        //    }
        //    catch (Exception e)
        //    {
        //        return Error(e);
        //    }
        //}

        ///// <summary>
        ///// Returns the interfaces that a given system can use
        ///// </summary>
        ///// <param name="itSystemId">The id of the system</param>
        ///// <param name="getCanUseInterfaces">flag</param>
        ///// <returns>List of interfaces</returns>
        //public HttpResponseMessage GetCanUseInterfaces(int itSystemId, bool? getCanUseInterfaces)
        //{
        //    try
        //    {
        //        var system = Repository.GetByKey(itSystemId);
        //        var interfaces = system.CanUseInterfaces;

        //        var dtos = Map(interfaces);
        //        return Ok(dtos);
        //    }
        //    catch (Exception e)
        //    {
        //        return Error(e);
        //    }
        //}

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
                return Error(e);
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
                return Error(e);
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
                return Error(e);
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

                var item = Map(dto);

                item.ObjectOwner = KitosUser;
                item.LastChangedByUser = KitosUser;

                foreach (var id in dto.TaskRefIds)
                {
                    var task = _taskRepository.GetByKey(id);
                    item.TaskRefs.Add(task);
                }
                
                PostQuery(item);

                return Created(Map(item), new Uri(Request.RequestUri + "/" + item.Id));
            }
            catch (Exception e)
            {
                return Error(e);
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
                return Error(e);
            }
        }

        public HttpResponseMessage PostTasksUsedByThisSystem(int id, [FromUri] int taskId)
        {
            try
            {
                var system = Repository.GetByKey(id);
                if (system == null) return NotFound();
                if (!HasWriteAccess(system)) return Unauthorized();

                var task = _taskRepository.GetByKey(taskId);
                if (task == null) return NotFound();

                system.TaskRefs.Add(task);

                system.LastChanged = DateTime.Now;
                system.LastChangedByUser = KitosUser;

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
                var system = Repository.GetByKey(id);
                if (system == null) return NotFound();
                if (!HasWriteAccess(system)) return Unauthorized();

                var task = _taskRepository.GetByKey(taskId);
                if (task == null) return NotFound();

                system.TaskRefs.Remove(task);

                system.LastChanged = DateTime.Now;
                system.LastChangedByUser = KitosUser;

                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
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

                //if a task group is given, only find the tasks in that group
                if (taskGroup.HasValue) pagingModel.Where(taskRef => taskRef.ParentId.Value == taskGroup.Value);
                else pagingModel.Where(taskRef => taskRef.Children.Count == 0);

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
                return Error(e);
            }
        }
        
        ///// <summary>
        ///// Adds a new can-use-interface to the system
        ///// </summary>
        ///// <param name="id">Id of the system</param>
        ///// <param name="interfaceId">Id of the interface, that can-be-used by the system</param>
        ///// <returns></returns>
        //public HttpResponseMessage PostInterfaceCanBeUsedBySystem(int id, [FromUri] int interfaceId)
        //{
        //    try
        //    {
        //        var system = Repository.GetByKey(id);
        //        if (system == null) return NotFound();
        //        if (!HasWriteAccess(system)) return Unauthorized();

        //        var theInterface = Repository.GetByKey(interfaceId);
        //        if (theInterface == null) return NotFound();

        //        system.CanUseInterfaces.Add(theInterface);
                
        //        //also update each of the existing SystemUsages to allow them to use the new interface
        //        foreach (var systemUsage in system.Usages)
        //        {
        //            _systemUsageService.AddInterfaceUsage(systemUsage, theInterface);
        //        }

        //        system.LastChanged = DateTime.Now;
        //        system.LastChangedByUser = KitosUser;

        //        Repository.Save();

        //        return Created(Map<ItSystem, ItSystemSimpleDTO>(theInterface));
        //    }
        //    catch (Exception e)
        //    {
        //        return Error(e);
        //    }
        //}

        //public HttpResponseMessage DeleteInterfaceCanBeUsedBySystem(int id, [FromUri] int interfaceId)
        //{
        //    try
        //    {
        //        var system = Repository.GetByKey(id);
        //        if (system == null) return NotFound();
        //        if (!HasWriteAccess(system)) return Unauthorized();

        //        var theInterface = _canUseInterfaceRepository.GetByKey(interfaceId);
        //        if (theInterface == null) return NotFound();

        //        system.CanUseInterfaces.Remove(theInterface);

        //        system.LastChanged = DateTime.Now;
        //        system.LastChangedByUser = KitosUser;

        //        Repository.Save();

        //        return Ok();
        //    }
        //    catch (Exception e)
        //    {
        //        return Error(e);
        //    }
        //}

        public override HttpResponseMessage Patch(int id, JObject obj)
        {
            // try get AccessModifier value
            JToken accessModToken;
            obj.TryGetValue("accessModifier", out accessModToken);
            // only global admin can set access mod to public
            if (accessModToken != null && accessModToken.ToObject<AccessModifier>() == AccessModifier.Public && !KitosUser.IsGlobalAdmin)
            {
                return Unauthorized();
            }
            return base.Patch(id, obj);
        }
    }
}
