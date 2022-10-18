using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationRelevantSystem
    {
        public int SystemId { get; set; }
        public IEnumerable<int> RelevantUnitIds { get; set; }

        public OrganizationRelevantSystem(int systemId, IEnumerable<int> relevantUnitIds)
        {
            SystemId = systemId;
            RelevantUnitIds = relevantUnitIds;
        }
    }
}
