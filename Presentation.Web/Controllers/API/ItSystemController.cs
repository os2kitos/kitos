using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.Model.System;
using Core.ApplicationServices.System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Newtonsoft.Json.Linq;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Presentation.Web.Models.ItSystem;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ItSystemController : GenericHierarchyApiController<ItSystem, ItSystemDTO>
    {
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IItSystemService _systemService;

        public ItSystemController(
            IGenericRepository<ItSystem> repository,
            IGenericRepository<TaskRef> taskRepository,
            IItSystemService systemService,
            IAuthorizationContext authorizationContext)
            : base(repository, authorizationContext)
        {
            _taskRepository = taskRepository;
            _systemService = systemService;
        }

        
        // DELETE api/T
        public override HttpResponseMessage Delete(int id, int organizationId)
        {
            var deleteResult = _systemService.Delete(id);
            switch (deleteResult)
            {
                case DeleteResult.Forbidden:
                    return Forbidden();
                case DeleteResult.NotFound:
                    return NotFound();
                case DeleteResult.InUse:
                case DeleteResult.HasChildren:
                case DeleteResult.HasInterfaceExhibits:
                    return DeleteConflict(deleteResult.MapToConflict().ToString("G"));
                case DeleteResult.Ok:
                    return Ok();  // Correct response would be NoContent, but somewhere in the frontend this breaks causing a double delete on one request.
                                  // This means the request fails with 500 since the system is already delete in the first pass
                default:
                    return Error($"Something went wrong trying to delete system with id: {id}");
            }
        }

        private HttpResponseMessage DeleteConflict(string response)
        {
            return CreateResponse(HttpStatusCode.Conflict, response, response);
        }

        protected override void DeleteQuery(ItSystem entity)
        {
            _systemService.Delete(entity.Id);
        }

        /// <summary>
        /// Henter alle IT-Systemer i organisationen samt offentlige IT Systemer fra andre organisationer
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="paging"></param>
        /// <param name="q">Mulighed for søgning på navneindhold</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItSystemDTO>>))]
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
                paging.WithPostProcessingFilter(AllowRead);

                if (!string.IsNullOrEmpty(q)) paging.Where(sys => sys.Name.Contains(q));

                var query = Page(Repository.AsQueryable(), paging);

                return Ok(Map(query));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItSystemDTO>>))]
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

                systems = systems.Where(AllowRead);

                var dtos = Map(systems);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItSystemDTO>>))]
        public HttpResponseMessage GetInterfacesSearch(string q, int orgId, bool? interfaces)
        {
            try
            {
                var systems = _systemService.GetInterfaces(orgId, q, KitosUser);

                systems = systems.Where(AllowRead);

                var dtos = Map(systems);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItSystemDTO>>))]
        public HttpResponseMessage GetNonInterfacesSearch(string q, int orgId, bool? nonInterfaces)
        {
            try
            {
                var systems = _systemService.GetNonInterfaces(orgId, q, KitosUser);

                systems = systems.Where(AllowRead);

                var dtos = Map(systems);
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItSystemDTO>>))]
        public HttpResponseMessage GetHierarchy(int id, [FromUri] bool hierarchy)
        {
            try
            {
                var systems = _systemService.GetHierarchy(id);

                systems = systems.Where(AllowRead);

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
                if (!IsAvailable(dto.Name, dto.OrganizationId))
                {
                    return Conflict("Name is already taken!");
                }

                var item = Map(dto);
                if (dto.AccessModifier == AccessModifier.Public && !AllowEntityVisibilityControl(item))
                {
                    return Forbidden();
                }

                item.ObjectOwner = KitosUser;
                item.LastChangedByUser = KitosUser;
                item.Uuid = Guid.NewGuid();

                if (!AllowCreate<ItSystem>(item))
                {
                    return Forbidden();
                }

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

        /// <summary>
        /// Henter alle IT Systemer ejet af organisationen samt alle IT systemer fra nadre organisationer som er taget i anvendelse.
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItSystemDTO>>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetItSystemsUsedByOrg([FromUri] int orgId)
        {
            try
            {
                var systems = Repository.Get(x => x.OrganizationId == orgId || x.Usages.Any(y => y.OrganizationId == orgId));

                systems = systems?.Where(AllowRead);

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
                if (!AllowModify(system))
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
                if (system == null)
                {
                    return NotFound();
                }

                if (!AllowModify(system))
                {
                    return Forbidden();
                }

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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<List<TaskRefSelectedDTO>>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetTasks(int id, bool? tasks, bool onlySelected, int? taskGroup, [FromUri] PagingModel<TaskRef> pagingModel)
        {
            try
            {
                var system = Repository.GetByKey(id);
                if (system == null)
                {
                    return NotFound();
                }

                if (!AllowRead(system))
                {
                    return Forbidden();
                }

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

                pagingModel.WithPostProcessingFilter(AllowRead);
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

            var itSystem = Repository.GetByKey(id);
            if (accessModToken != null && accessModToken.ToObject<AccessModifier>() == AccessModifier.Public && !AllowEntityVisibilityControl(itSystem))
            {
                return Forbidden();
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

        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Conflict, Description = "It System names must be new")]
        public HttpResponseMessage GetNameAvailable(string checkname, int orgId)
        {
            try
            {
                if (GetOrganizationReadAccessLevel(orgId) == OrganizationDataReadAccessLevel.None)
                {
                    return Forbidden();
                }
                return IsAvailable(checkname, orgId) ? Ok() : Conflict("Name is already taken!");
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [HttpGet]
        [Route("api/v1/ItSystem/{id}/usingOrganizations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<UsingOrganizationDTO>>))]
        public HttpResponseMessage GetUsingOrganizations([FromUri] int id)
        {
            var itSystemUsages = _systemService.GetUsingOrganizations(id);
            switch (itSystemUsages.Status)
            {
                case OperationResult.Forbidden:
                    return Forbidden();
                case OperationResult.NotFound:
                    return NotFound();
                case OperationResult.Ok:
                    var dto = Map(itSystemUsages.Value);
                    return Ok(dto);
                default:
                    return CreateResponse(HttpStatusCode.InternalServerError,
                        "An error occured when trying to get using organizations");
            }

        }

        private bool IsAvailable(string name, int orgId)
        {
            var system = Repository.Get(x => x.Name == name && x.OrganizationId == orgId);
            return !system.Any();
        }

        private static IEnumerable<UsingOrganizationDTO> Map(IEnumerable<UsingOrganization> usingOrganizations)
        {
            return usingOrganizations.Select(
                usingOrganization => new UsingOrganizationDTO
                {
                    SystemUsageId = usingOrganization.ItSystemUsageId,
                    Organization = usingOrganization.Organization.MapToNamedEntityDTO()
                })
                .ToList();
        }
    }
}
