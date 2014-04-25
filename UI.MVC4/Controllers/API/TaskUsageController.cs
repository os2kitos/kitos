using System;
using System.Linq;
using System.Net.Http;
using System.Security;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class TaskUsageController : GenericApiController<TaskUsage, int, TaskUsageDTO>
    {
        private readonly IOrgUnitService _orgUnitService;

        public TaskUsageController(IGenericRepository<TaskUsage> repository,IOrgUnitService orgUnitService) 
            : base(repository)
        {
            _orgUnitService = orgUnitService;
        }

        public HttpResponseMessage Get(int orgUnitId)
        {
            return Get(orgUnitId, false);
        }

        public HttpResponseMessage Get(int orgUnitId, bool onlyStarred)
        {
            var usages = Repository.Get(u => u.OrgUnitId == orgUnitId);

            if (onlyStarred) usages = usages.Where(u => u.Starred);

            var result = usages.Select(CompileDelegation);

            return Ok(result);
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
            var usage = Repository.Get(u => u.OrgUnitId == unit.Id && u.TaskRefId == task.Id).FirstOrDefault();
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

            if (!_orgUnitService.HasWriteAccess(KitosUser, unit)) throw new SecurityException();

            Repository.DeleteByKey(entity.Id);
            DeleteTaskOnChildren(unit, taskRefId);

            Repository.Save();
        }

        protected override TaskUsage PostQuery(TaskUsage item)
        {

            var unit = item.OrgUnit;

            if (!_orgUnitService.HasWriteAccess(KitosUser, unit)) throw new SecurityException();

            return base.PostQuery(item);
        }

        public override HttpResponseMessage Put(int id, TaskUsageDTO dto)
        {
            return NotAllowed();
        }

        public override HttpResponseMessage Patch(int id, Newtonsoft.Json.Linq.JObject obj)
        {

            var entity = Repository.GetByKey(id);
            if (!_orgUnitService.HasWriteAccess(KitosUser, entity.OrgUnit)) return Unauthorized();

            return base.Patch(id, obj);
        }
    }
}
