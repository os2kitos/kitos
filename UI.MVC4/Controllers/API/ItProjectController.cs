using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItProjectController : GenericApiController<ItProject, int, ItProjectDTO>
    {
        private readonly IItProjectService _itProjectService;
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IGenericRepository<Organization> _orgRepository;

        public ItProjectController(IGenericRepository<ItProject> repository, IItProjectService itProjectService, IGenericRepository<Organization> orgRepository, IGenericRepository<TaskRef> taskRepository)
            : base(repository)
        {
            _itProjectService = itProjectService;
            _orgRepository = orgRepository;
            _taskRepository = taskRepository;
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
    }
}