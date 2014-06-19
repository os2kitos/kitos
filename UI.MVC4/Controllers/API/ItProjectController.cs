using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItProjectController : GenericHasRightsController<ItProject, ItProjectRight, ItProjectRole, ItProjectDTO>
    {
        private readonly IItProjectService _itProjectService;
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IGenericRepository<ItSystemUsage> _itSystemUsageRepository;
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;

        //TODO: Man, this constructor smells ...
        public ItProjectController(IGenericRepository<ItProject> repository, IGenericRepository<ItProjectRight> rightRepository,
            IItProjectService itProjectService, IGenericRepository<OrganizationUnit> orgUnitRepository, IGenericRepository<TaskRef> taskRepository, 
            IGenericRepository<ItSystemUsage> itSystemUsageRepository) 
            : base(repository, rightRepository)
        {
            _itProjectService = itProjectService;
            _taskRepository = taskRepository;
            _itSystemUsageRepository = itSystemUsageRepository;
            _orgUnitRepository = orgUnitRepository;
        }

        public HttpResponseMessage GetByOrg([FromUri] int orgId)
        {
            try
            {
                var projects = _itProjectService.GetAll(orgId, includePublic: false).ToList();
                
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

        public HttpResponseMessage GetCatalog(bool? catalog, [FromUri] int orgId)
        {
            try
            {
                //Get all projects inside the organizaton OR public
                var projects = _itProjectService.GetAll(orgId, includePublic: true);

                var dto = Map<IEnumerable<ItProject>, IEnumerable<ItProjectTypeDTO>>(projects);

                return Ok(dto);
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

        public HttpResponseMessage GetProjectsByCategory([FromUri] int orgId, [FromUri] int catId)
        {
            try
            {
                var projects = _itProjectService.GetAll(orgId, includePublic:false).Where(p => p.ItProjectTypeId == catId);

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
                var project = _itProjectService.GetAll().FirstOrDefault(p => p.Id == id);

                if (project == null) return NotFound();

                return Ok(Map<IEnumerable<OrganizationUnit>, IEnumerable<OrgUnitDTO>>(project.UsedByOrgUnits));
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
                
                project.UsedByOrgUnits.Add(orgUnit);
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

                var orgUnit = _orgUnitRepository.GetByKey(organizationUnit);
                if (orgUnit == null) return NotFound();
                
                project.UsedByOrgUnits.Remove(orgUnit);
                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetTasksUsedByThisProject(int id, [FromUri] int taskId)
        {
            try
            {
                var project = Repository.GetByKey(id);

                if (project == null) return NotFound();

                return Ok(Map<IEnumerable<TaskRef>, IEnumerable<TaskRefDTO>>(project.TaskRefs));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage PostTasksUsedByThisProject(int id, [FromUri] int taskId)
        {
            try
            {
                var project = Repository.GetByKey(id); 
                if (project == null) return NotFound();

                if (!HasWriteAccess(project)) return Unauthorized();

                var task = _taskRepository.GetByKey(taskId);
                if (task == null) return NotFound();
                
                project.TaskRefs.Add(task);
                Repository.Save();

                return Created(Map<TaskRef, TaskRefDTO>(task));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage DeleteTasksUsedByThisProject(int id, [FromUri] int taskId)
        {
            try
            {
                var project = Repository.GetByKey(id); 
                if (project == null) return NotFound();

                if (!HasWriteAccess(project)) return Unauthorized();

                var task = _taskRepository.GetByKey(taskId);
                if (task == null) return NotFound();

                project.TaskRefs.Remove(task);
                Repository.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetItSystemsUsedByThisProject(int id, [FromUri] bool itSystems)
        {
            try
            {
                var project = Repository.GetByKey(id); 
                if (project == null) return NotFound();
                
                return Ok(Map<IEnumerable<ItSystem>, IEnumerable<ItSystemDTO>>(project.ItSystemUsages.Select(x => x.ItSystem)));
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
    }
}