using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V1.Users
{
    public class TransferRightsRequestDTO
    {
        [Required]
        public int ToUserId { get; set; }
        [Required]
        public IEnumerable<AssignedRightDTO> RightsToTransfer { get; set; }
    }
}