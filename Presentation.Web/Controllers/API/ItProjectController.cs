using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Core.ApplicationServices;
using Core.ApplicationServices.Project;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ItProjectController : GenericHierarchyApiController<ItProject, ItProjectDTO>
    {
        private readonly IItProjectService _itProjectService;
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IGenericRepository<ItSystemUsage> _itSystemUsageRepository;
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;

        public ItProjectController(
            IGenericRepository<ItProject> repository,
            IItProjectService itProjectService,
            IGenericRepository<OrganizationUnit> orgUnitRepository,
            IGenericRepository<TaskRef> taskRepository,
            IGenericRepository<ItSystemUsage> itSystemUsageRepository)
            : base(repository)
        {
            _itProjectService = itProjectService;
            _taskRepository = taskRepository;
            _itSystemUsageRepository = itSystemUsageRepository;
            _orgUnitRepository = orgUnitRepository;
        }

        /// <summary>
        /// Henter alle IT-Projekter i organisationen samt offentlige IT-projekter fra andre organisationer
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="pagingModel"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItProjectDTO>>))]
        public HttpResponseMessage GetByOrg([FromUri] int orgId, [FromUri] PagingModel<ItProject> pagingModel)
        {
            try
            {
                var projectsQuery = _itProjectService.GetAvailableProjects(orgId);

                var projects = Page(projectsQuery, pagingModel);

                return Ok(Map(projects));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        /// <summary>
        /// Henter alle IT-Projekter i organisationen samt offentlige IT-projekter fra andre organisationer
        /// </summary>
        /// <param name="q"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItProjectDTO>>))]
        public virtual HttpResponseMessage Get(string q, int orgId, int take = 25)
        {
            try
            {
                var projectsQuery = _itProjectService
                    .GetAvailableProjects(orgId, q)
                    .OrderBy(_=>_.Name)
                    .Take(take);

                return Ok(Map(projectsQuery));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItProjectDTO>>))]
        public HttpResponseMessage GetHierarchy(int id, [FromUri] bool? hierarchy)
        {
            try
            {
                var itProject = Repository.AsQueryable().ById(id);

                if (itProject == null)
                    return NotFound();

                if (!AllowRead(itProject))
                {
                    return Forbidden();
                }

                // this trick will put the first object in the result as well as the children
                var children = new[] { itProject }.SelectNestedChildren(x => x.Children);
                // gets parents only
                var parents = itProject.SelectNestedParents(x => x.Parent);
                // put it all in one result
                var contracts = children.Union(parents);
                return Ok(Map(contracts));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItProjectDTO>>))]
        public HttpResponseMessage GetProjectsByType([FromUri] int orgId, [FromUri] int typeId)
        {
            try
            {
                var projects = Repository
                    .AsQueryable()
                    .ByOrganizationId(orgId)
                    .Where(p => p.ItProjectTypeId == typeId)
                    .AsEnumerable()
                    .Where(AllowRead)
                    .ToList();

                return Ok(Map(projects));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage PostOrganizationUnitsUsingThisProject(int id, int organizationId, [FromUri] int organizationUnit)
        {
            try
            {
                var project = Repository.GetByKey(id);
                if (project == null)
                {
                    return NotFound();
                }

                if (!AllowModify(project))
                {
                    return Forbidden();
                }

                var orgUnit = _orgUnitRepository.GetByKey(organizationUnit);

                if (orgUnit == null)
                {
                    return BadRequest("Invalid org unit id");
                }

                project.UsedByOrgUnits.Add(new ItProjectOrgUnitUsage { ItProjectId = id, OrganizationUnitId = organizationUnit });

                Repository.Save();

                return Created(Map<OrganizationUnit, OrgUnitDTO>(orgUnit));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        /// <summary>
        /// Removes an Organization Unit from the project.UsedByOrgUnits list.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organizationId"></param>
        /// <param name="organizationUnit"></param>
        /// <returns></returns>
        public HttpResponseMessage DeleteOrganizationUnitsUsingThisProject(int id, int organizationId, [FromUri] int organizationUnit)
        {
            try
            {
                var project = Repository.GetByKey(id);
                if (project == null)
                {
                    return NotFound();
                }

                if (!AllowModify(project))
                {
                    return Forbidden();
                }

                var entity = project.UsedByOrgUnits.SingleOrDefault(x => x.ItProjectId == id && x.OrganizationUnitId == organizationUnit);
                if (entity == null)
                {
                    return BadRequest("Org unit not found");
                }
                project.UsedByOrgUnits.Remove(entity);

                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage PostTaskToProject(int id, int organizationId, [FromUri] int? taskId)
        {
            try
            {
                var project = Repository.GetByKey(id);
                if (project == null)
                {
                    return NotFound();
                }

                if (!AllowModify(project))
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
                            x.ItProjects.All(y => y.Id != id)).ToList();
                }
                else
                {
                    // no taskId was specified so get everything
                    tasks = _taskRepository.Get(
                        x =>
                            !x.Children.Any() &&
                            x.ItProjects.All(y => y.Id != id)).ToList();
                }

                if (!tasks.Any())
                {
                    return BadRequest("No tasks found for the provided ids");
                }

                foreach (var task in tasks)
                {
                    project.TaskRefs.Add(task);
                }
                Repository.Save();
                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage DeleteTaskToProject(int id, int organizationId, [FromUri] int? taskId)
        {
            try
            {
                var project = Repository.GetByKey(id);
                if (project == null)
                {
                    return NotFound();
                }

                if (!AllowModify(project))
                {
                    return Forbidden();
                }

                List<TaskRef> tasks;
                if (taskId.HasValue)
                {
                    tasks =
                        project.TaskRefs.Where(
                            x =>
                                (x.Id == taskId || x.ParentId == taskId || x.Parent.ParentId == taskId) &&
                                !x.Children.Any())
                            .ToList();
                }
                else
                {
                    // no taskId was specified so get everything
                    tasks = project.TaskRefs.ToList();
                }

                if (!tasks.Any())
                {
                    return BadRequest("No tasks found for the provided ids");
                }

                foreach (var task in tasks)
                {
                    project.TaskRefs.Remove(task);
                }
                Repository.Save();
                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        /// <summary>
        /// Returns a list of task ref and whether or not they are currently associated with a given project
        /// </summary>
        /// <param name="id">ID of the project</param>
        /// <param name="tasks">Routing qualifer</param>
        /// <param name="onlySelected">If true, only return those task ref that are associated with the project. If false, return all task ref.</param>
        /// <param name="taskGroup">Optional filtering on task group</param>
        /// <param name="pagingModel">Paging model</param>
        /// <returns>List of TaskRefSelectedDTO</returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<List<TaskRefSelectedDTO>>))]
        public HttpResponseMessage GetTasks(int id, bool? tasks, bool onlySelected, int? taskGroup, [FromUri] PagingModel<TaskRef> pagingModel)
        {
            try
            {
                var project = Repository.GetByKey(id);

                if (project == null)
                {
                    return NotFound();
                }

                if (!AllowRead(project))
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

                //if a task group is given, only find the tasks in that group
                if (taskGroup.HasValue)
                    pagingModel.Where(taskRef => (taskRef.ParentId.Value == taskGroup.Value ||
                                                  taskRef.Parent.ParentId.Value == taskGroup.Value) &&
                                                 !taskRef.Children.Any());
                else
                    pagingModel.Where(taskRef => taskRef.Children.Count == 0);

                var theTasks = Page(taskQuery, pagingModel).ToList();

                var dtos = theTasks.Select(task => new TaskRefSelectedDTO()
                {
                    TaskRef = Map<TaskRef, TaskRefDTO>(task),
                    IsSelected = onlySelected || project.TaskRefs.Any(t => t.Id == task.Id)
                }).ToList(); // must call .ToList here else the output will be wrapped in $type,$values

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItSystemUsageDTO>>))]
        public HttpResponseMessage GetItSystemsUsedByThisProject(int id, [FromUri] bool? usages)
        {
            try
            {
                var project = Repository.GetByKey(id);
                if (project == null) return NotFound();

                if (!AllowRead(project))
                {
                    return Forbidden();
                }

                return Ok(Map<IEnumerable<ItSystemUsage>, IEnumerable<ItSystemUsageDTO>>(project.ItSystemUsages));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage PostItSystemsUsedByThisProject(int id, int organizationId, [FromUri] int usageId)
        {
            try
            {
                var project = Repository.GetByKey(id);

                if (project == null)
                {
                    return NotFound();
                }

                if (!AllowModify(project))
                {
                    return Forbidden();
                }

                var systemUsage = _itSystemUsageRepository.GetByKey(usageId);
                if (systemUsage == null)
                {
                    return BadRequest("System usage not found");
                }

                project.ItSystemUsages.Add(systemUsage);

                Repository.Save();

                return Created(Map<ItSystemUsage, ItSystemUsageDTO>(systemUsage));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage DeleteItSystemsUsedByThisProject(int id, int organizationId, [FromUri] int usageId)
        {
            try
            {
                var project = Repository.GetByKey(id);

                if (project == null)
                {
                    return NotFound();
                }

                if (!AllowModify(project))
                {
                    return Forbidden();
                }

                var systemUsage = _itSystemUsageRepository.GetByKey(usageId);
                if (systemUsage == null)
                {
                    return BadRequest("Usage not found");
                }

                project.ItSystemUsages.Remove(systemUsage);
                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        /// <summary>  
        ///  Accessmodifier is and should always be 0 since it is not allowed to be accessed outside the organisation.
        /// </summary>
        protected override ItProject PostQuery(ItProject item)
        {
            var result = _itProjectService.AddProject(item.Name, item.OrganizationId);
            if (result.Ok)
                return result.Value;

            if (result.Error == OperationFailure.Forbidden)
                throw new SecurityException();
            throw new InvalidOperationException(result.Error.ToString("G"));
        }

        protected override void DeleteQuery(ItProject entity)
        {
            var result = _itProjectService.DeleteProject(entity.Id);
            if (!result.Ok)
            {
                if (result.Error == OperationFailure.Forbidden)
                    throw new SecurityException();
                throw new InvalidOperationException(result.Error.ToString("G"));
            }
        }

        public HttpResponseMessage PostPhaseChange(int id, int organizationId, string phaseNum, JObject phaseObj)
        {
            var project = Repository.GetByKey(id);

            if (project == null)
            {
                return NotFound();
            }
            if (!AllowModify(project))
            {
                return Forbidden();
            }

            const string propertyName = "Phase";
            var phaseRef = project.GetType().GetProperty(propertyName + phaseNum);
            // make sure object has the property we are after
            if (phaseRef != null)
            {
                var phase = phaseRef.GetValue(project);

                foreach (var valuePair in phaseObj)
                {
                    var propRef = phase.GetType().GetProperty(valuePair.Key);
                    var t = propRef.PropertyType;
                    var jToken = valuePair.Value;
                    try
                    {
                        // get reference to the generic method obj.Value<t>(parameter);
                        var genericMethod = jToken.GetType().GetMethod("Value").MakeGenericMethod(t);
                        // use reflection to call obj.Value<t>("keyName");
                        var value = genericMethod.Invoke(phaseObj, new object[] { valuePair.Key });
                        // update the entity
                        propRef.SetValue(phase, value);
                    }
                    catch (Exception)
                    {
                        // if obj.Value<t>("keyName") cast fails set to fallback value
                        propRef.SetValue(phase, null); // TODO this is could be dangerous, should probably also be default(t)
                    }
                }
            }

            //force set access modifier to 0
            project.AccessModifier = AccessModifier.Local;

            PatchQuery(project, null);

            return Ok();
        }

        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        {
            obj.TryGetValue("accessModifier", out var accessModToken);
            var itProject = Repository.GetByKey(id);
            if (itProject == null)
            {
                return NotFound();
            }

            if (accessModToken != null && accessModToken.ToObject<AccessModifier>() == AccessModifier.Public && AllowEntityVisibilityControl(itProject) == false)
            {
                return Forbidden();
            }

            return base.Patch(id, organizationId, obj);
        }
    }
}
