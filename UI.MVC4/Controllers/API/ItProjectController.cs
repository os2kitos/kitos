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
    public class ItProjectController : GenericApiController<ItProject, int, ItProjectDTO>
    {
        private readonly IItProjectService _itProjectService;
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IGenericRepository<ItSystemUsage> _itSystemUsageRepository;
        private readonly IGenericRepository<Organization> _orgRepository;
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;

        public ItProjectController(IGenericRepository<ItProject> repository, IItProjectService itProjectService, IGenericRepository<Organization> orgRepository, IGenericRepository<OrganizationUnit> orgUnitRepository, IGenericRepository<TaskRef> taskRepository, IGenericRepository<ItSystemUsage> itSystemUsageRepository) 
            : base(repository)
        {
            _itProjectService = itProjectService;
            _orgRepository = orgRepository;
            _taskRepository = taskRepository;
            _itSystemUsageRepository = itSystemUsageRepository;
            _orgUnitRepository = orgUnitRepository;
        }

        public HttpResponseMessage GetByOrg([FromUri] int orgId)
        {
            try
            {
                var projects = Repository.Get(x => x.AccessModifier == AccessModifier.Public || x.OrganizationId == orgId).ToList();

                var clonedParentIds = projects.Where(x => x.ParentItProjectId.HasValue).Select(x => x.ParentItProjectId);

                // remove cloned parents
                projects.RemoveAll(x => clonedParentIds.Contains(x.Id));
                
                return Ok(Map(projects));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetPrograms(string q, int orgId, bool? programs)
        {
            try
            {
                //TODO: check for user read access rights

                var org = _orgRepository.GetByKey(orgId);

                var thePrograms = _itProjectService.GetPrograms(org, q);

                return Ok(Map(thePrograms));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetNonPrograms(string q, int orgId, bool? nonPrograms)
        {
            try
            {
                //TODO: check for user read access rights

                var org = _orgRepository.GetByKey(orgId);

                var projects = _itProjectService.GetProjects(org, q);

                return Ok(Map(projects));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage PostCloneProject(int id, [FromBody] ItProjectDTO dto)
        {
            try
            {
                var project = Repository.GetByKey(id);

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
                var projects = Repository.Get(x => x.OrganizationId == orgId && x.ItProjectCategoryId == catId);

                return projects == null ? NotFound() : Ok(Map(projects));
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
                var usage = Repository.GetByKey(id);

                if (usage == null) return NotFound();

                return Ok(Map<IEnumerable<OrganizationUnit>, IEnumerable<OrgUnitDTO>>(usage.UsedByOrgUnits));
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
                var usage = Repository.GetByKey(id);
                var orgUnit = _orgUnitRepository.GetByKey(organizationUnit);

                if (usage == null || orgUnit == null) return NotFound();

                usage.UsedByOrgUnits.Add(orgUnit);
                Repository.Save();

                return Created(Map<OrganizationUnit, OrgUnitDTO>(orgUnit));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage DeleteOrganizationUnitsUsingThisProject(int id, [FromUri] int organizationUnit)
        {
            try
            {
                var usage = Repository.GetByKey(id);
                var orgUnit = _orgUnitRepository.GetByKey(organizationUnit);

                if (usage == null || orgUnit == null) return NotFound();

                usage.UsedByOrgUnits.Remove(orgUnit);
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
                var usage = Repository.GetByKey(id);

                if (usage == null) return NotFound();

                return Ok(Map<IEnumerable<TaskRef>, IEnumerable<TaskRefDTO>>(usage.TaskRefs));
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
                var usage = Repository.GetByKey(id);
                var task = _taskRepository.GetByKey(taskId);

                if (usage == null || task == null) return NotFound();

                usage.TaskRefs.Add(task);
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
                var usage = Repository.GetByKey(id);
                var task = _taskRepository.GetByKey(taskId);

                if (usage == null || task == null) return NotFound();

                usage.TaskRefs.Remove(task);
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
                var itProject = Repository.GetByKey(id);

                if (itProject == null) return NotFound();

                return Ok(Map<IEnumerable<ItSystem>, IEnumerable<ItSystemDTO>>(itProject.ItSystemUsages.Select(x => x.ItSystem)));
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
                var itProject = Repository.GetByKey(id);
                var usage = _itSystemUsageRepository.GetByKey(usageId);

                if (itProject == null || usage == null) return NotFound();

                itProject.ItSystemUsages.Add(usage);
                Repository.Save();

                return Created(Map<ItSystemUsage, ItSystemUsageDTO>(usage));
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
                var itProject = Repository.GetByKey(id);
                var usage = _itSystemUsageRepository.GetByKey(usageId);

                if (itProject == null || usage == null) return NotFound();

                itProject.ItSystemUsages.Remove(usage);
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
                var projects = Repository.Get(x => x.OrganizationId == orgId || x.UsedByOrgUnits.Any(y => y.OrganizationId == orgId));

                if (projects == null) return NotFound();

                return Ok(Map(projects));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        /// <summary>
        /// Used to set checked state in available project list
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="usageId"></param>
        /// <returns></returns>
        public HttpResponseMessage GetItProjectsUsedByOrg([FromUri] int orgId, [FromUri] int usageId)
        {
            try
            {
                var projects = Repository.Get(x => x.OrganizationId == orgId && x.ItSystemUsages.Any(y => y.Id == usageId));

                if (projects == null) return NotFound();

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
            _itProjectService.AddProject(item);
            return item;
        }
    }
}