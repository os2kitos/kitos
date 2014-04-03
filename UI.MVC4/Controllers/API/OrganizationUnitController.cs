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
            TaskRefNotifyParentAdd(orgUnit, task);

            Repository.Save();
            return Ok(); // TODO figure out what to return when refs are posted
        }

        public HttpResponseMessage DeleteTaskRef(int id, [FromUri] int taskRef)
        {
            var task = _taskRepository.GetByKey(taskRef);
            var orgUnit = Repository.GetByKey(id);

            RemoveTaskTree(orgUnit, task);
            TaskRefNotifyParentRemove(orgUnit, task);

            Repository.Save();
            return NoContent(); // TODO figure out what to return when refs are posted
        }


        //call this when removing a task to notify the parent-task, that the child has changed
        private void TaskRefNotifyParentRemove(OrganizationUnit unit, TaskRef task)
        {
            var parent = task.Parent;
            if (parent == null) return;

            //remove the parent task
            unit.TaskRefs.Remove(parent);

            //add every child task - except the one, which was just removed
            foreach (var child in parent.Children)
            {
                if(child != task) AddTask(unit, child);
            }

            //since the parent was now removed, we should notify the parent's parent
            TaskRefNotifyParentRemove(unit, parent);
        }


        //call this when adding a task to notify the parent-task, that the child has changed
        private void TaskRefNotifyParentAdd(OrganizationUnit unit, TaskRef task)
        {
            var parent = task.Parent;
            if (parent == null) return;

            //check if all children has been selected
            var allChildrenSelected = parent.Children.All(t => HasTask(unit, t));

            if (!allChildrenSelected) return;

            //if we get this far, every child of parent has been selected
            //add parent task
            AddTask(unit, parent);

            //remove children task, which are now superfluous
            foreach (var child in parent.Children)
            {
                RemoveTaskTree(unit, child);
            }

            Repository.Update(unit);

            //next, notify parent's parent
            TaskRefNotifyParentAdd(unit, parent);
        }

        //add a task 
        private void AddTask(OrganizationUnit unit, TaskRef task)
        {
            if(!HasTask(unit, task)) unit.TaskRefs.Add(task);

            Repository.Update(unit);
        }

        //remove a task and all its children
        private void RemoveTaskTree(OrganizationUnit unit, TaskRef task)
        {
            var unvisited = new Queue<TaskRef>();
            unvisited.Enqueue(task);

            foreach (var taskRef in unvisited)
            {
                //remove task ref on the unit, if it exist
                if (unit.TaskRefs.Contains(taskRef))
                {
                    unit.TaskRefs.Remove(taskRef);
                    //since taskref is on the unit, we know that none of its children will be, so we're done with this subtree
                }
                else
                {
                    //if task ref wasn't on the unit, maybe some of it's children are
                    foreach (var child in taskRef.Children)
                    {
                        unvisited.Enqueue(child);
                    }
                }
            }

            Repository.Update(unit);
        }

        //check if a given unit has selected that task
        private bool HasTask(OrganizationUnit unit, TaskRef task)
        {
            return unit.TaskRefs.Contains(task);
        }
    }
}
