using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1.Users
{
    public class RemoveUserRightsRequest
    {
        public IEnumerable<AssignedRightDTO> RightsToRemove { get; set; }
    }
}