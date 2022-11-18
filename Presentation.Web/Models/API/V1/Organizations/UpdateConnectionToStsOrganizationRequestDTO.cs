using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class UpdateConnectionToStsOrganizationRequestDTO : ConnectToStsOrganizationRequestDTO
    {
        [Required]
        public Guid UserUuid { get; set; }
    }
}