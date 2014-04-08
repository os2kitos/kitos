using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class TaskUsageController : GenericApiController<TaskUsage, int, TaskUsageDTO>
    {
        private readonly IGenericRepository<TaskRef> _taskRepository;
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;

        public TaskUsageController(IGenericRepository<TaskUsage> repository, IGenericRepository<TaskRef> taskRepository, IGenericRepository<OrganizationUnit> orgUnitRepository) : base(repository)
        {
            _taskRepository = taskRepository;
            _orgUnitRepository = orgUnitRepository;
        }

        public HttpResponseMessage Get(int orgUnitId)
        {
            var usages = Repository.Get(x => x.OrgUnitId == orgUnitId);

            var delegations = new List<TaskDelegationDTO>();
            foreach (var usage in usages)
            {
                //access to foreach closure ...
                var temp = usage;

                var childUsages = Repository.Get(x => x.TaskRefId == temp.TaskRefId && x.OrgUnit.ParentId == orgUnitId);

                delegations.Add(new TaskDelegationDTO
                    {
                        ParentUsage = Map(usage),
                        ChildrenUsage = Map<IEnumerable<TaskUsage>, IEnumerable<TaskUsageDTO>>(childUsages)
                    });                
            }

            return Ok(delegations);
        }

        public override HttpResponseMessage Post(TaskUsageDTO dto)
        {
            try
            {
                var task = _taskRepository.GetByKey(dto.TaskRefId);
                var orgUnit = _orgUnitRepository.GetByKey(dto.OrgUnitId);

                //only add a task, if the parent org unit also has it
                if (orgUnit.Parent != null && !HasTaskRecursive(orgUnit.Parent, task))
                {
                    return Conflict("Parent OrgUnit must have task usage, before it can be applied to this OrgUnit");
                }

                //removed every selected subtask
                RemoveTaskTree(orgUnit, task);
                //add this task - using the dto, so we can persist extra values like starred and statuses
                AddUsage(dto);
                //notify parent
                AddedUsageNotifyParent(orgUnit, task);

                Repository.Save();
                return Ok(); // TODO figure out what to return when refs are posted
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage DeleteTaskRef(int id, [FromUri] int taskRef)
        {
            try
            {
                var task = _taskRepository.GetByKey(taskRef);
                var orgUnit = _orgUnitRepository.GetByKey(id);

                //we gotta remove the task on this unit and every sub orgUnit, so
                //breadth first traversal of the org unit tree
                var unvisitedUnits = new Queue<OrganizationUnit>();
                unvisitedUnits.Enqueue(orgUnit);

                while (unvisitedUnits.Count > 0)
                {
                    var unit = unvisitedUnits.Dequeue();

                    //removed this task and all subtasks
                    RemoveTaskTree(unit, task);
                    //notify parent
                    RemovedUsageNotifyParent(unit, task);

                    //do the same for every org unit child
                    foreach (var child in unit.Children)
                    {
                        unvisitedUnits.Enqueue(child);
                    }


                    Repository.Save();

                }

                return NoContent(); // TODO figure out what to return when refs are posted
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        private void AddUsage(OrganizationUnit unit, TaskRef task)
        {
            if (Repository.Get(usage => usage.OrgUnitId == unit.Id && usage.TaskRefId == task.Id).Any()) return;

            Repository.Insert(new TaskUsage()
            {
                OrgUnit = unit,
                TaskRef = task
            });
            Repository.Save();
        }

        private void AddUsage(TaskUsageDTO dto)
        {
            if (Repository.Get(u => u.OrgUnitId == dto.OrgUnitId && u.TaskRefId == dto.TaskRefId).Any()) return;

            Repository.Insert(Map(dto));
            Repository.Save();
        }

        private void RemoveUsage(OrganizationUnit unit, TaskRef task)
        {
            var usage = Repository.Get(u => u.OrgUnitId == unit.Id && u.TaskRefId == task.Id).First();

            Repository.DeleteByKey(usage.Id);
        }


        //call this when removing a task usage to notify the parent-task, that the child has changed
        private void RemovedUsageNotifyParent(OrganizationUnit unit, TaskRef task)
        {
            var parent = task.Parent;
            if (parent == null) return;

            //since a child of the parent has been deselected, the parent is deselected as well,
            //so remove the parent task if it was previously selected (directly or indirectly through an ancestor)
            if (HasTaskRecursive(unit, parent))
            {
                RemoveUsage(unit, parent);

                //add every child task - except the one, which was just removed
                foreach (var child in parent.Children)
                {
                    if (child != task) AddUsage(unit, child);
                }

                //since the parent was now removed, we should notify the parent's parent
                RemovedUsageNotifyParent(unit, parent);
            }
        }

        //call this when adding a task usage to notify the parent-task, that the child has changed
        private void AddedUsageNotifyParent(OrganizationUnit unit, TaskRef task)
        {
            var parent = task.Parent;
            if (parent == null) return;

            //check if all children has been selected
            var allChildrenSelected = parent.Children.All(t => HasTask(unit, t));

            if (!allChildrenSelected) return;

            //if we get this far, every child of parent has been selected
            //add parent task
            AddUsage(unit, parent);

            //remove children task, which are now superfluous
            foreach (var child in parent.Children)
            {
                RemoveTaskTree(unit, child);
            }

            Repository.Update(unit);

            //next, notify parent's parent
            AddedUsageNotifyParent(unit, parent);
        }

        //remove a task and all its children.
        private void RemoveTaskTree(OrganizationUnit unit, TaskRef task)
        {
            var unvisited = new Queue<TaskRef>();
            unvisited.Enqueue(task);

            while (unvisited.Count > 0)
            {
                var taskRef = unvisited.Dequeue();

                //remove task ref on the unit, if it exist
                if (HasTask(unit, taskRef))
                {
                    unit.TaskRefs.Remove(taskRef);
                    //since taskref is on the unit, we know that none of its children will be, so we're done with this subtree
                }

                //if task ref wasn't on the unit, maybe some of it's children are
                foreach (var child in taskRef.Children)
                {
                    unvisited.Enqueue(child);
                }
            }

            Repository.Update(unit);
        }

        //return true if a given unit has selected that task
        private bool HasTask(OrganizationUnit unit, TaskRef task)
        {
            return Repository.Get(usage => usage.TaskRef == task && usage.OrgUnit == unit).Any();
        }

        //return true if a given unit has selected that task, or an ancestor task
        private bool HasTaskRecursive(OrganizationUnit unit, TaskRef task)
        {
            while (task != null)
            {
                //access to modified closure
                var tmp = task;

                if (Repository.Get(usage => usage.TaskRef == tmp && usage.OrgUnit == unit).Any()) 
                    return true;

                task = task.Parent;
            }

            return false;
        }
    }
}
