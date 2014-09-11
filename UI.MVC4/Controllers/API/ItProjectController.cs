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
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItProjectController : GenericHierarchyApiController<ItProject, ItProjectDTO>
    {
        private readonly IItProjectService _itProjectService;
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IGenericRepository<ItSystemUsage> _itSystemUsageRepository;
        private readonly IGenericRepository<ItProjectRole> _roleRepository;
        private readonly IGenericRepository<ItProjectPhase> _phaseRepository;
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;

        //TODO: Man, this constructor smells ...
        public ItProjectController(
            IGenericRepository<ItProject> repository,
            IItProjectService itProjectService, 
            IGenericRepository<OrganizationUnit> orgUnitRepository, 
            IGenericRepository<TaskRef> taskRepository, 
            IGenericRepository<ItSystemUsage> itSystemUsageRepository,
            IGenericRepository<ItProjectRole> roleRepository,
            IGenericRepository<ItProjectPhase> phaseRepository)
            : base(repository)
        {
            _itProjectService = itProjectService;
            _taskRepository = taskRepository;
            _itSystemUsageRepository = itSystemUsageRepository;
            _roleRepository = roleRepository;
            _phaseRepository = phaseRepository;
            _orgUnitRepository = orgUnitRepository;
        }

        public HttpResponseMessage GetByOrg([FromUri] int orgId, [FromUri] PagingModel<ItProject> pagingModel)
        {
            try
            {
                //Get all projects inside the organizaton
                pagingModel.Where(p => p.OrganizationId == orgId);

                var projects = Page(Repository.AsQueryable(), pagingModel);

                return Ok(Map(projects));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public virtual HttpResponseMessage Get(string q, int orgId)
        {
            try
            {
                var items = Repository.Get(x => x.Name.Contains(q) && x.OrganizationId == orgId);

                return Ok(Map(items));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

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
                return Error(e);
            }
        }

        public HttpResponseMessage GetOverview(bool? overview, [FromUri] int orgId, [FromUri] string q, [FromUri] PagingModel<ItProject> pagingModel)
        {
            try
            {
                //Get all projects inside the organizaton
                pagingModel.Where(p => p.OrganizationId == orgId);
                if (!string.IsNullOrEmpty(q)) pagingModel.Where(proj => proj.Name.Contains(q));

                var projects = Page(Repository.AsQueryable(), pagingModel);

                var dtos = Map<IEnumerable<ItProject>, IEnumerable<ItProjectOverviewDTO>>(projects);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetExcel(bool? csv, [FromUri] int orgId, [FromUri] string q, [FromUri] PagingModel<ItProject> pagingModel)
        {
            try
            {
                //Get all projects inside the organizaton
                pagingModel.Where(p => p.OrganizationId == orgId);
                if (!string.IsNullOrEmpty(q)) pagingModel.Where(proj => proj.Name.Contains(q));

                var projects = Page(Repository.AsQueryable(), pagingModel);

                var dtos = Map<IEnumerable<ItProject>, IEnumerable<ItProjectOverviewDTO>>(projects);

                var roles = _roleRepository.Get().ToList();
                var phases = _phaseRepository.Get().ToList();

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
                                String.Join(",", project.Rights.Where(x => x.RoleId == roleId).Select(x => x.User.Name)));
                    }
                    obj.Add("ID", project.ItProjectId);
                    obj.Add("Type", project.ItProjectTypeName);
                    obj.Add("Strategisk", project.IsStrategy);
                    obj.Add("Tværgaaende", project.IsTransversal);
                    obj.Add("Fase", phases.SingleOrDefault(x => x.Id == project.CurrentPhaseId));
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
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "itprojektoversigt.csv" };
                return result;
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

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
                return Error(e);
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
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "itprojektoversigt.csv" };
                return result;
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage PostCloneProject(int id, bool? clone, [FromBody] ItProjectDTO dto)
        {
            try
            {
                //make sure we only clone projects that the is accessible from the organization
                var project = _itProjectService.GetAll(dto.OrganizationId).FirstOrDefault(p => p.Id == id);

                var clonedProject = _itProjectService.CloneProject(project, KitosUser, dto.OrganizationId);

                return Created(Map(clonedProject), new Uri(Request.RequestUri + "/" + clonedProject.Id));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetProjectsByType([FromUri] int orgId, [FromUri] int typeId)
        {
            try
            {
                var projects = _itProjectService.GetAll(orgId, includePublic:false).Where(p => p.ItProjectTypeId == typeId);

                return Ok(Map(projects));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetOrganizationUnitsUsingThisProject(int id, [FromUri] int organizationUnit)
        {
            try
            {
                var project = Repository.GetByKey(id);
                if (project == null) return NotFound();

                var dtos =
                    Map<IEnumerable<OrganizationUnit>, IEnumerable<OrgUnitDTO>>(
                        project.UsedByOrgUnits.Select(x => x.OrganizationUnit));

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage PostOrganizationUnitsUsingThisProject(int id, [FromUri] int organizationUnit)
        {
            try
            {
                var project = Repository.GetByKey(id);
                if (project == null) return NotFound();

                if (!HasWriteAccess(project)) return Unauthorized();

                var orgUnit = _orgUnitRepository.GetByKey(organizationUnit);
                if (orgUnit == null) return NotFound();
                
                project.UsedByOrgUnits.Add(new ItProjectOrgUnitUsage {ItProjectId = id, OrganizationUnitId = organizationUnit});
                
                project.LastChanged = DateTime.Now;
                project.LastChangedByUser = KitosUser;

                Repository.Save();

                return Created(Map<OrganizationUnit, OrgUnitDTO>(orgUnit));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        /// <summary>
        /// Removes an Organization Unit from the project.UsedByOrgUnits list.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="organizationUnit"></param>
        /// <returns></returns>
        public HttpResponseMessage DeleteOrganizationUnitsUsingThisProject(int id, [FromUri] int organizationUnit)
        {
            try
            {
                var project = Repository.GetByKey(id);
                if (project == null) return NotFound();

                if(!HasWriteAccess(project)) return Unauthorized();

                var entity = project.UsedByOrgUnits.SingleOrDefault(x => x.ItProjectId == id && x.OrganizationUnitId == organizationUnit);
                if (entity == null) return NotFound();
                project.UsedByOrgUnits.Remove(entity);

                project.LastChanged = DateTime.Now;
                project.LastChangedByUser = KitosUser;

                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }
        
        public HttpResponseMessage PostTaskToProject(int id, [FromUri] int taskId)
        {
            try
            {
                var project = Repository.GetByKey(id); 
                if (project == null) return NotFound();

                if (!HasWriteAccess(project)) return Unauthorized();

                var task = _taskRepository.GetByKey(taskId);
                if (task == null) return NotFound();
                
                project.TaskRefs.Add(task);

                project.LastChanged = DateTime.Now;
                project.LastChangedByUser = KitosUser;

                Repository.Save();

                return Created(Map<TaskRef, TaskRefDTO>(task));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage DeleteTasksToProject(int id, [FromUri] int taskId)
        {
            try
            {
                var project = Repository.GetByKey(id); 
                if (project == null) return NotFound();

                if (!HasWriteAccess(project)) return Unauthorized();

                var task = _taskRepository.GetByKey(taskId);
                if (task == null) return NotFound();

                project.TaskRefs.Remove(task);

                project.LastChanged = DateTime.Now;
                project.LastChangedByUser = KitosUser;

                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
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
                if (taskGroup.HasValue) pagingModel.Where(taskRef => taskRef.ParentId.Value == taskGroup.Value);
                else pagingModel.Where(taskRef => taskRef.Children.Count == 0);

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
                return Error(e);
            }
        }

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
                return Error(e);
            }
        }

        public HttpResponseMessage PostItSystemsUsedByThisProject(int id, [FromUri] int usageId)
        {
            try
            {
                var project = Repository.GetByKey(id); 
                if (project == null) return NotFound();

                if (!HasWriteAccess(project)) return Unauthorized();

                //TODO: should also we check for write access to the system usage?
                var systemUsage = _itSystemUsageRepository.GetByKey(usageId);
                if (systemUsage == null) return NotFound();
                
                project.ItSystemUsages.Add(systemUsage);

                project.LastChanged = DateTime.Now;
                project.LastChangedByUser = KitosUser;

                Repository.Save();

                return Created(Map<ItSystemUsage, ItSystemUsageDTO>(systemUsage));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage DeleteItSystemsUsedByThisProject(int id, [FromUri] int usageId)
        {
            try
            {
                var project = Repository.GetByKey(id); 
                if (project == null) return NotFound();

                if (!HasWriteAccess(project)) return Unauthorized();

                var systemUsage = _itSystemUsageRepository.GetByKey(usageId);

                if (systemUsage == null) return NotFound();
                
                project.ItSystemUsages.Remove(systemUsage);

                project.LastChanged = DateTime.Now;
                project.LastChangedByUser = KitosUser;

                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        /// <summary>
        /// Used to list all available projects
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="itProjects"></param>
        /// <returns></returns>
        public HttpResponseMessage GetItProjectsUsedByOrg([FromUri] int orgId, [FromUri] bool itProjects)
        {
            try
            {
                var projects = _itProjectService.GetAll(orgId, includePublic: false);

                //TODO: if list is empty, return empty list, not NotFound()
                if (projects == null) return NotFound();

                return Ok(Map(projects));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        /// <summary>
        /// Used to set checked state in available project list in ItSystemUsage
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="usageId"></param>
        /// <returns></returns>
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
                return Error(e);
            }
        }
        
        protected override ItProject PostQuery(ItProject item)
        {
            //Makes sure to create the necessary properties, like phases
            return _itProjectService.AddProject(item);
        }

        protected override void DeleteQuery(int id)
        {
            var project = Repository.GetByKey(id);

            _itProjectService.DeleteProject(project);
        }

        public override HttpResponseMessage Post(ItProjectDTO dto)
        {
            // only global admin can set access mod to public
            if (dto.AccessModifier == AccessModifier.Public && !KitosUser.IsGlobalAdmin)
            {
                return Unauthorized();
            }
            return base.Post(dto);
        }

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
