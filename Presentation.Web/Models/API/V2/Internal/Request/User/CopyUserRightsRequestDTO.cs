using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Request.User
{
    public class CopyUserRightsRequestDTO
    {
        public IEnumerable<CopyRightRequestDTO> UnitRights {get; set; }

        public IEnumerable<CopyRightRequestDTO> SystemRights {get; set; }

        public IEnumerable<CopyRightRequestDTO> ContractRights {get; set; }

        public IEnumerable<CopyRightRequestDTO> DataProcessingRights {get; set; }
        
    }
}