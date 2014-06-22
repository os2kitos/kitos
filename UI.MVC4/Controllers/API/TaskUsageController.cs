using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class TaskUsageController : GenericApiController<TaskUsage, TaskUsageDTO>
    {

        public TaskUsageController(IGenericRepository<TaskUsage> repository) 
            : base(repository)
        {
        }

        public HttpResponseMessage Get(int orgUnitId)
        {
            return Get(orgUnitId, false);
        }

        public HttpResponseMessage Get(int orgUnitId, bool onlyStarred)
        {
            try
            {
                var usages = Repository.Get(u => u.OrgUnitId == orgUnitId);

                if (onlyStarred) usages = usages.Where(u => u.Starred);

                var delegationDtos = usages.Select(CompileDelegation);

                return Ok(delegationDtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetProjects(int id, bool? projects)
        {
            try
            {
                var usage = Repository.GetByKey(id);

                var theProjects = usage.TaskRef.ItProjects.Where(p => p.OrganizationId == usage.OrgUnit.OrganizationId);
                var dtos = Map<IEnumerable<ItProject>, IEnumerable<ItProjectSimpleDTO>>(theProjects);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetSystems(int id, bool? systems)
        {
            try
            {
                var taskUsage = Repository.GetByKey(id);


                var indirectUsages =
                    taskUsage.TaskRef.ItSystems.SelectMany(system => system.Usages)
                             .Where(usage => usage.OrganizationId == taskUsage.OrgUnit.OrganizationId);

                var directUsages =
                    taskUsage.TaskRef.ItSystemUsages.Where(
                        usage => usage.OrganizationId == taskUsage.OrgUnit.OrganizationId);

                var allUsages = indirectUsages.Union(directUsages);

                var dtos = Map<IEnumerable<ItSystemUsage>, IEnumerable<ItSystemUsageSimpleDTO>>(allUsages);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        //Given a task usage, compile the task delegation down the org unit tree
        private TaskDelegationDTO CompileDelegation(TaskUsage usage)
        {
            if (usage == null) throw new ArgumentNullException();

            var delegations = usage.OrgUnit.Children.Select(child => CompileDelegation(child, usage.TaskRef))
                                   .Where(childDelegation => childDelegation != null).ToList();

            var delegation = new TaskDelegationDTO()
            {
                Usage = Map(usage),
                Delegations = delegations,
                HasDelegations = delegations.Any()
            };

            return delegation;
        }

        //Given a unit and a task, compile the task delegation down the org unit tree
        private TaskDelegationDTO CompileDelegation(OrganizationUnit unit, TaskRef task)
        {
            var unitId = unit.Id;
            var taskId = task.Id;

            var usage = Repository.Get(u => u.OrgUnitId == unitId && u.TaskRefId == taskId).FirstOrDefault();
            if (usage == null) return null;

            var delegations = unit.Children.Select(child => CompileDelegation(child, task))
                                  .Where(childDelegation => childDelegation != null).ToList();

            var delegation = new TaskDelegationDTO()
                {
                    Usage = Map(usage),
                    Delegations = delegations,
                    HasDelegations = delegations.Any()
                };

            return delegation;
        }

        private void DeleteTaskOnChildren(OrganizationUnit orgUnit, int taskRefId)
        {
            foreach (var unit in orgUnit.Children)
            {
                var temp = unit;
                var usages = Repository.Get(u => u.TaskRefId == taskRefId && u.OrgUnitId == temp.Id);

                foreach (var taskUsage in usages)
                {
                    Repository.DeleteByKey(taskUsage.Id);
                }

                DeleteTaskOnChildren(unit, taskRefId);
            }
        }

        protected override void DeleteQuery(int id)
        {
            var entity = Repository.GetByKey(id);

            var taskRefId = entity.TaskRefId;
            var unit = entity.OrgUnit;

            Repository.DeleteByKey(entity.Id);
            DeleteTaskOnChildren(unit, taskRefId);

            Repository.Save();
        }
        
        public override HttpResponseMessage Put(int id, JObject jObject)
        {
            return NotAllowed();
        }
    }
}
