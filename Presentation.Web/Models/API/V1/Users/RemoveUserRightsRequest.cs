using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.DomainModel.Organization;

namespace Presentation.Web.Models.API.V1.Users
{
    public class RemoveUserRightsRequest
    {
        [Required]
        public IEnumerable<OrganizationRole> AdminRoles { get; set; }
        [Required]
        public IEnumerable<AssignedRightDTO> BusinessRights { get; set; }
    }
}