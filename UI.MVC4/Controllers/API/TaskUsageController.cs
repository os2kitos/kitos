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
        public TaskUsageController(IGenericRepository<TaskUsage> repository) 
            : base(repository)
        {
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
    }
}
