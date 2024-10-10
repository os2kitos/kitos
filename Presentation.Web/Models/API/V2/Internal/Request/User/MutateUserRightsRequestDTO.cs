using System.Collections.Generic;
namespace Presentation.Web.Models.API.V2.Internal.Request.User
{
    public class MutateUserRightsRequestDTO
    {
        public IEnumerable<MutateRightRequestDTO> UnitRights { get; set; }
        public IEnumerable<MutateRightRequestDTO> SystemRights { get; set; }
        public IEnumerable<MutateRightRequestDTO> ContractRights { get; set; }
        public IEnumerable<MutateRightRequestDTO> DataProcessingRights { get; set; }
    }
}
