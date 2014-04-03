using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class OrganizationUnitController : GenericApiController<OrganizationUnit, int, OrgUnitDTO>
    {
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IOrgUnitService _orgUnitService;

        public OrganizationUnitController(IGenericRepository<OrganizationUnit> repository, IGenericRepository<TaskRef> taskRepository, IOrgUnitService orgUnitService) 
            : base(repository)
        {
            _taskRepository = taskRepository;
            _orgUnitService = orgUnitService;
        }

        public HttpResponseMessage GetByUser(int userId)
        {
            try
            {
                var user = KitosUser;

                if(user.Id != userId) throw new SecurityException();

                var orgUnits = _orgUnitService.GetByUser(user);

                return Ok(Map<ICollection<OrganizationUnit>, ICollection<OrgUnitDTO>>(orgUnits));

            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetByOrganization(int organization)
        {
            try
            {
                var orgUnit = Repository.Get(o => o.Organization_Id == organization && o.Parent == null).FirstOrDefault();

                if (orgUnit == null) return NotFound();

                var item = Map<OrganizationUnit, OrgUnitDTO>(orgUnit);

                return Ok(item);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetTaskRefs(int id, [FromUri] bool? taskRefs)
        {
            var refs = Repository.Get(x => x.Id == id).SelectMany(x => x.TaskRefs);
            return Ok(Map<IEnumerable<TaskRef>, IEnumerable<TaskOrgUnitRefDTO>>(refs));
        }

        public HttpResponseMessage PostTaskRef(int id, [FromUri] int taskRef)
        {
            var task = _taskRepository.GetByKey(taskRef);
            var orgUnit = Repository.GetByKey(id);

            AddTask(orgUnit, task);
            TaskRefNotifyParent(orgUnit, task);

            Repository.Save();
            return Ok(); // TODO figure out what to return when refs are posted
        }

        public HttpResponseMessage DeleteTaskRef(int id, [FromUri] int taskRef)
        {
            var task = _taskRepository.GetByKey(taskRef);
            var orgUnit = Repository.GetByKey(id);

            RemoveTask(orgUnit, task);
            TaskRefNotifyParent(orgUnit, task);

            Repository.Save();
            return NoContent(); // TODO figure out what to return when refs are posted
        }

        //recursively notify the parent of a task, that something has changed
        private void TaskRefNotifyParent(OrganizationUnit unit, TaskRef task)
        {
            var parent = task.Parent;
            if (parent == null) return;

            var parentStatus = parent.Children.All(t => HasTask(unit, t));

            if (parentStatus)
            {
                if (!HasTask(unit, parent)) unit.TaskRefs.Add(parent);
            }
            else
            {
                unit.TaskRefs.Remove(parent);
            }

            Repository.Update(unit);

            //next, notify parent's parent
            TaskRefNotifyParent(unit, parent);
        }

        //add a task and all its children
        private void AddTask(OrganizationUnit unit, TaskRef task)
        {
            var unvisited = new Queue<TaskRef>();
            unvisited.Enqueue(task);

            foreach (var taskRef in unvisited)
            {
                if(!HasTask(unit, task)) unit.TaskRefs.Add(task);

                foreach (var child in taskRef.Children)
                {
                    unvisited.Enqueue(child);
                }
            }

            Repository.Update(unit);
        }

        //remove a task and all its children
        private void RemoveTask(OrganizationUnit unit, TaskRef task)
        {
            var unvisited = new Queue<TaskRef>();
            unvisited.Enqueue(task);

            foreach (var taskRef in unvisited)
            {
                unit.TaskRefs.Remove(task);

                foreach (var child in taskRef.Children)
                {
                    unvisited.Enqueue(child);
                }
            }

            Repository.Update(unit);
        }

        //non-recursively check if a given unit has selected that task
        private bool HasTask(OrganizationUnit unit, TaskRef task)
        {
            return unit.TaskRefs.Contains(task);
        }
    }
}
