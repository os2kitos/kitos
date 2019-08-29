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
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;
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
        private readonly IGenericRepository<ItProjectRole> _roleRepository;
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;

        //TODO: Man, this constructor smells ...
        public ItProjectController(
            IGenericRepository<ItProject> repository,
            IItProjectService itProjectService,
            IGenericRepository<OrganizationUnit> orgUnitRepository,
            IGenericRepository<TaskRef> taskRepository,
            IGenericRepository<ItSystemUsage> itSystemUsageRepository,
            IGenericRepository<ItProjectRole> roleRepository)
            : base(repository)
        {
            _itProjectService = itProjectService;
            _taskRepository = taskRepository;
            _itSystemUsageRepository = itSystemUsageRepository;
            _roleRepository = roleRepository;
            _orgUnitRepository = orgUnitRepository;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItProjectDTO>>))]
        public HttpResponseMessage GetByOrg([FromUri] int orgId, [FromUri] PagingModel<ItProject> pagingModel)
        {
            try
            {
                //Get all projects inside the organizaton
                pagingModel.Where(
                    p =>
                        // it's public everyone can see it
                        p.AccessModifier == AccessModifier.Public ||
                        // or limit all to within the context
                        p.OrganizationId == orgId &&
                        // global admin sees all
                        (KitosUser.IsGlobalAdmin ||
                        // object owner sees his own objects
                        p.ObjectOwnerId == KitosUser.Id ||
                        // everyone in the same organization can see local objects
                        p.AccessModifier == AccessModifier.Local)
                    );

                var projects = Page(Repository.AsQueryable(), pagingModel);

                return Ok(Map(projects));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }
        /// <summary>  
        ///  Accessmodifier is and should always be 0 since it is not allowed to be accessed outside the organisation
        /// </summary>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItProjectDTO>>))]
        public virtual HttpResponseMessage Get(string q, int orgId)
        {
            try
            {
                var items = Repository.Get(
                    p =>
                        // filter by project name
                        p.Name.Contains(q) &&
                        // it's public everyone can see it
                        (p.AccessModifier == AccessModifier.Public ||
                        // or limit all to within the context
                        p.OrganizationId == orgId) &&
                        // global admin sees all within the context
                        (KitosUser.IsGlobalAdmin ||
                        // object owner sees his own objects
                        p.ObjectOwnerId == KitosUser.Id ||
                        // everyone in the same organization can see local objects
                        p.AccessModifier == AccessModifier.Local)
                    );

                return Ok(Map(items));
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
                var itProject = Repository.AsQueryable().Single(x => x.Id == id);

                if (itProject == null)
                    return NotFound();

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

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItProjectOverviewDTO>>))]
        public HttpResponseMessage GetOverview(bool? overview, [FromUri] int orgId, [FromUri] string q, [FromUri] PagingModel<ItProject> pagingModel)
        {
            try
            {
                //Get all projects which the user has access to
                pagingModel.Where(
                    p =>
                        // global admin sees all within the context
                        KitosUser.IsGlobalAdmin && p.OrganizationId == orgId ||
                        // object owner sees his own objects within the context
                        p.ObjectOwnerId == KitosUser.Id && p.OrganizationId == orgId ||
                        // it's public everyone can see it
                        p.AccessModifier == AccessModifier.Public ||
                        // everyone in the same organization can see local objects
                        p.AccessModifier == AccessModifier.Local && p.OrganizationId == orgId
                    );

                if (!string.IsNullOrEmpty(q)) pagingModel.Where(proj => proj.Name.Contains(q));

                var projects = Page(Repository.AsQueryable(), pagingModel);

                var dtos = Map<IEnumerable<ItProject>, IEnumerable<ItProjectOverviewDTO>>(projects);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetExcel(bool? csv, [FromUri] int orgId)
        {
            try
            {
                //Get all projects inside the organizaton
                var projects = Repository.Get(p => p.OrganizationId == orgId);

                //if (!string.IsNullOrEmpty(q)) pagingModel.Where(proj => proj.Name.Contains(q));
                //var projects = Page(Repository.AsQueryable(), pagingModel);

                var dtos = Map<IEnumerable<ItProject>, IEnumerable<ItProjectOverviewDTO>>(projects);

                var roles = _roleRepository.Get().ToList();

                var list = new List<dynamic>();
                var header = new ExpandoObject() as IDictionary<string, Object>;
                header.Add("Arkiveret", "Arkiveret");
                header.Add("Name", "It Projekt");
                header.Add("OrgUnit", "Ansv. organisationsenhed");
                foreach (var role in roles)
                    header.Add(role.Name, role.Name);
                header.Add("ID", "Projekt ID");
                header.Add("Type", "Type");
                header.Add("Strategisk", "Strategisk");
                header.Add("Tværgaaende", "Tværgående");
                header.Add("Fase", "Fase");
                header.Add("Status", "Status projekt");
                header.Add("Maal", "Status mål");
                header.Add("Risiko", "Risiko");
                header.Add("RO", "RO");
                header.Add("Okonomi", "Økonomi");
                header.Add("P1", "Prioritet 1");
                header.Add("P2", "Prioritet 2");
                list.Add(header);
                foreach (var project in dtos)
                {
                    var obj = new ExpandoObject() as IDictionary<string, Object>;
                    obj.Add("Arkiveret", project.IsArchived);
                    obj.Add("Name", project.Name);
                    obj.Add("OrgUnit", project.ResponsibleOrgUnitName);

                    foreach (var role in roles)
                    {
                        var roleId = role.Id;
                        obj.Add(role.Name,
                                String.Join(",", project.Rights.Where(x => x.RoleId == roleId).Select(x => x.User.FullName)));
                    }
                    obj.Add("ID", project.ItProjectId);
                    obj.Add("Type", project.ItProjectTypeName);
                    obj.Add("Strategisk", project.IsStrategy);
                    obj.Add("Tværgaaende", project.IsTransversal);

                    switch (project.CurrentPhase)
                    {
                        case 1:
                            obj.Add("Fase", project.Phase1.Name);
                            break;
                        case 2:
                            obj.Add("Fase", project.Phase2.Name);
                            break;
                        case 3:
                            obj.Add("Fase", project.Phase3.Name);
                            break;
                        case 4:
                            obj.Add("Fase", project.Phase4.Name);
                            break;
                        case 5:
                            obj.Add("Fase", project.Phase5.Name);
                            break;
                        default:
                            obj.Add("Fase", "Ikke sat");
                            break;
                    }

                    obj.Add("Status", project.StatusProject);
                    obj.Add("Maal", project.GoalStatusStatus);
                    obj.Add("Risiko", project.AverageRisk);
                    obj.Add("RO", project.Roi);
                    obj.Add("Okonomi", project.Bc);
                    obj.Add("P1", project.Priority);
                    obj.Add("P2", project.PriorityPf);
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
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileNameStar = "itprojektoversigt.csv", DispositionType = "ISO-8859-1" };
                return result;
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItProjectCatalogDTO>>))]
        public HttpResponseMessage GetCatalog(bool? catalog, [FromUri] int orgId, [FromUri] string q, [FromUri] PagingModel<ItProject> pagingModel)
        {
            try
            {
                //Get all projects inside the organizaton OR public
                pagingModel.Where(p => p.OrganizationId == orgId || p.AccessModifier == AccessModifier.Public);
                if (!string.IsNullOrEmpty(q)) pagingModel.Where(proj => proj.Name.Contains(q));

                var projects = Page(Repository.AsQueryable(), pagingModel);

                var dtos = Map<IEnumerable<ItProject>, IEnumerable<ItProjectCatalogDTO>>(projects);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetExcelCat(bool? csvcat, [FromUri] int orgId, [FromUri] string q, [FromUri] PagingModel<ItProject> pagingModel)
        {
            try
            {
                //Get all projects inside the organizaton OR public
                pagingModel.Where(p => p.OrganizationId == orgId || p.AccessModifier == AccessModifier.Public);
                if (!string.IsNullOrEmpty(q)) pagingModel.Where(proj => proj.Name.Contains(q));

                var projects = Page(Repository.AsQueryable(), pagingModel);

                var dtos = Map<IEnumerable<ItProject>, IEnumerable<ItProjectCatalogDTO>>(projects);

                var list = new List<dynamic>();
                var header = new ExpandoObject() as IDictionary<string, Object>;
                header.Add("Name", "It Projekt");
                header.Add("Org", "Oprettet af: Organisation");
                header.Add("Navn", "Oprettet af: Navn");
                header.Add("ID", "Projekt ID");
                header.Add("Type", "Type");
                header.Add("Public", "Public");
                header.Add("Arkiv", "Arkiv");
                list.Add(header);
                foreach (var project in dtos)
                {
                    var obj = new ExpandoObject() as IDictionary<string, Object>;
                    obj.Add("Name", project.Name);
                    obj.Add("Org", project.OrganizationName);
                    obj.Add("Navn", project.ObjectOwnerName);
                    obj.Add("ID", project.ItProjectId);
                    obj.Add("Type", project.ItProjectTypeName);
                    obj.Add("Public", project.AccessModifier);
                    obj.Add("Arkiv", project.IsArchived);
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
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileNameStar = "itprojektoversigt.csv", DispositionType = "ISO-8859-1" };
                return result;
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
                var projects = _itProjectService.GetAll(orgId, includePublic:false).Where(p => p.ItProjectTypeId == typeId);

                return Ok(Map(projects));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<OrgUnitDTO>>))]
        public HttpResponseMessage GetOrganizationUnitsUsingThisProject(int id, [FromUri] int organizationUnit)
        {
            try
            {
                var project = Repository.GetByKey(id);
                if (project == null)
                {
                    return NotFound();
                }

                var dtos =
                    Map<IEnumerable<OrganizationUnit>, IEnumerable<OrgUnitDTO>>(
                        project.UsedByOrgUnits.Select(x => x.OrganizationUnit));

                return Ok(dtos);
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

                if (!HasWriteAccess(project, organizationId))
                {
                    return Forbidden();
                }

                var orgUnit = _orgUnitRepository.GetByKey(organizationUnit);
                if (orgUnit == null)
                {
                    return NotFound();
                }

                project.UsedByOrgUnits.Add(new ItProjectOrgUnitUsage {ItProjectId = id, OrganizationUnitId = organizationUnit});

                project.LastChanged = DateTime.UtcNow;
                project.LastChangedByUser = KitosUser;

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

                if (!HasWriteAccess(project, organizationId))
                {
                    return Forbidden();
                }

                var entity = project.UsedByOrgUnits.SingleOrDefault(x => x.ItProjectId == id && x.OrganizationUnitId == organizationUnit);
                if (entity == null)
                {
                    return NotFound();
                }
                project.UsedByOrgUnits.Remove(entity);

                project.LastChanged = DateTime.UtcNow;
                project.LastChangedByUser = KitosUser;

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
                if (!HasWriteAccess(project, organizationId))
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
                            x.ItProjects.All(y => y.Id != id)).ToList();
                }
                else
                {
                    // no taskId was specified so get everything
                    tasks = _taskRepository.Get(
                        x =>
                            !x.Children.Any() &&
                            x.AccessModifier == AccessModifier.Public &&
                            x.ItProjects.All(y => y.Id != id)).ToList();
                }

                if (!tasks.Any())
                {
                    return NotFound();
                }

                foreach (var task in tasks)
                {
                    project.TaskRefs.Add(task);
                }
                project.LastChanged = DateTime.UtcNow;
                project.LastChangedByUser = KitosUser;
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

                if (!HasWriteAccess(project, organizationId))
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
                    return NotFound();
                }

                foreach (var task in tasks)
                {
                    project.TaskRefs.Remove(task);
                }
                project.LastChanged = DateTime.UtcNow;
                project.LastChangedByUser = KitosUser;
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
                                                 !taskRef.Children.Any() &&
                                                 taskRef.AccessModifier == AccessModifier.Public); // TODO add support for normal
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

                if (!HasWriteAccess(project, organizationId))
                {
                    return Forbidden();
                }

                //TODO: should also we check for write access to the system usage?
                var systemUsage = _itSystemUsageRepository.GetByKey(usageId);
                if (systemUsage == null)
                {
                    return NotFound();
                }

                project.ItSystemUsages.Add(systemUsage);

                project.LastChanged = DateTime.UtcNow;
                project.LastChangedByUser = KitosUser;

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

                if (!HasWriteAccess(project, organizationId))
                {
                    return Forbidden();
                }

                var systemUsage = _itSystemUsageRepository.GetByKey(usageId);
                if (systemUsage == null)
                {
                    return NotFound();
                }

                project.ItSystemUsages.Remove(systemUsage);
                project.LastChanged = DateTime.UtcNow;
                project.LastChangedByUser = KitosUser;
                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        /// <summary>
        /// Used to list all available projects
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="itProjects"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItProjectDTO>>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetItProjectsUsedByOrg([FromUri] int orgId, [FromUri] bool itProjects)
        {
            try
            {
                var projects = _itProjectService.GetAll(orgId, includePublic: false);
                // TODO: if list is empty, return empty list, not NotFound()
                return projects == null ? NotFound() : Ok(Map(projects));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        /// <summary>
        /// Used to set checked state in available project list in ItSystemUsage
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="usageId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<IEnumerable<ItProjectDTO>>))]
        public HttpResponseMessage GetItProjectsUsedByOrg([FromUri] int orgId, [FromUri] int usageId)
        {
            try
            {
                var projects = _itProjectService.GetAll(orgId, includePublic: false)
                    .Where(project => project.ItSystemUsages.Any(usage => usage.Id == usageId)).ToList();

                return Ok(Map(projects));
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
            //Makes sure to create the necessary properties, like phases
            //force set access modifier to 0
            item.AccessModifier = 0;
            return _itProjectService.AddProject(item);
        }

        protected override void DeleteQuery(ItProject entity)
        {
            _itProjectService.DeleteProject(entity.Id);
        }

        public override HttpResponseMessage Post(ItProjectDTO dto)
        {
            // only global admin can set access mod to public
            if (dto.AccessModifier == AccessModifier.Public && !FeatureChecker.CanExecute(KitosUser, Feature.CanSetAccessModifierToPublic))
            {
                return Forbidden();
            }
            //force set access modifier to 0
            dto.AccessModifier = 0;
            return base.Post(dto);
        }

        public HttpResponseMessage PostPhaseChange(int id, int organizationId, string phaseNum, JObject phaseObj)
        {
            var project = Repository.GetByKey(id);

            if (project == null)
            {
                return NotFound();
            }
            if (!HasWriteAccess(project, organizationId))
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

            project.LastChanged = DateTime.UtcNow;
            project.LastChangedByUser = KitosUser;

            //force set access modifier to 0
            project.AccessModifier = 0;

            PatchQuery(project, null);

            return Ok();
        }

        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        {
            // try get AccessModifier value
            JToken accessModToken;
            obj.TryGetValue("accessModifier", out accessModToken);
            // only global admin can set access mod to public

            if (accessModToken != null && accessModToken.ToObject<AccessModifier>() == AccessModifier.Public &&
                !FeatureChecker.CanExecute(KitosUser, Feature.CanSetAccessModifierToPublic))
            {
                return Forbidden();
            }
            return base.Patch(id, organizationId, obj);
        }

        protected override bool HasWriteAccess(ItProject obj, User user, int organizationId)
        {
            // local admin have write access if the obj is in context
            if (obj.IsInContext(organizationId) &&
                user.OrganizationRights.Any(x => x.OrganizationId == organizationId && (x.Role == OrganizationRole.LocalAdmin || x.Role == OrganizationRole.ProjectModuleAdmin)))
                return true;

            return base.HasWriteAccess(obj, user, organizationId);
        }
    }
}
