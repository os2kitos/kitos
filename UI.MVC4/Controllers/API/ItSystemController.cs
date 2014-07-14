using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
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

        public HttpResponseMessage GetPublic([FromUri] int organizationId, [FromUri] PagingModel<ItSystem> paging)
        {
            try
            {
                var systems =
                    Repository.AsQueryable()
                              .Where(sys => sys.AccessModifier == AccessModifier.Public || sys.BelongsToId == organizationId);
                var query = Page(systems, paging);

                return Ok(Map(query));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        /// <summary>
        /// Returns the interfaces that a given system exposes
        /// </summary>
        /// <param name="itSystemId">The id of the exposing system</param>
        /// <param name="getExposedInterfaces">flag</param>
        /// <returns>List of interfaces</returns>
        public HttpResponseMessage GetExposedInterfaces(int itSystemId, bool? getExposedInterfaces)
        {
            try
            {
                var interfaces = Repository.Get(system => system.ExposedById == itSystemId);
                var dtos = Map(interfaces);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }


        /// <summary>
        /// Returns the interfaces that a given system can use
        /// </summary>
        /// <param name="itSystemId">The id of the system</param>
        /// <param name="getCanUseInterfaces">flag</param>
        /// <returns>List of interfaces</returns>
        public HttpResponseMessage GetCanUseInterfaces(int itSystemId, bool? getCanUseInterfaces)
        {
            try
            {
                var system = Repository.GetByKey(itSystemId);
                var interfaces = system.CanUseInterfaces;

                var dtos = Map(interfaces);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetInterfaces(bool? interfaces)
        {
            try
            {
                var systems = _systemService.GetInterfaces(null, null);
                var dtos = Map(systems);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetInterfacesSearch(string q, bool? interfaces)
        {
            try
            {
                var systems = _systemService.GetInterfaces(null, q);
                var dtos = Map(systems);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetNonInterfaces(bool? nonInterfaces)
        {
            try
            {
                var systems = _systemService.GetNonInterfaces(null, null);
                var dtos = Map(systems);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetNonInterfacesSearch(string q, bool? nonInterfaces)
        {
            try
            {
                var systems = _systemService.GetNonInterfaces(null, q);
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
                var item = Map(dto);

                item.ObjectOwner = KitosUser;
                item.LastChangedByUser = KitosUser;

                foreach (var dataRow in item.DataRows)
                {
                    dataRow.ObjectOwner = KitosUser;
                    dataRow.LastChangedByUser = KitosUser;
                }

                foreach (var id in dto.TaskRefIds)
                {
                    var task = _taskRepository.GetByKey(id);
                    item.TaskRefs.Add(task);
                }

                foreach (var id in dto.CanUseInterfaceIds)
                {
                    var intrface = Repository.GetByKey(id);
                    item.CanUseInterfaces.Add(intrface);
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
                });

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage PostInterfaceCanBeUsedBySystem(int id, [FromUri] int interfaceId)
        {
            try
            {
                var system = Repository.GetByKey(id);
                if (system == null) return NotFound();
                if (!HasWriteAccess(system)) return Unauthorized();

                var theInterface = Repository.GetByKey(interfaceId);
                if (theInterface == null) return NotFound();

                system.CanUseInterfaces.Add(theInterface);

                system.LastChanged = DateTime.Now;
                system.LastChangedByUser = KitosUser;

                Repository.Save();

                return Created(Map<ItSystem, ItSystemSimpleDTO>(theInterface));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage DeleteInterfaceCanBeUsedBySystem(int id, [FromUri] int interfaceId)
        {
            try
            {
                var system = Repository.GetByKey(id);
                if (system == null) return NotFound();
                if (!HasWriteAccess(system)) return Unauthorized();

                var theInterface = Repository.GetByKey(interfaceId);
                if (theInterface == null) return NotFound();

                system.CanUseInterfaces.Remove(theInterface);

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
    }
}
