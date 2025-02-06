using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1
{
    public class UserDTO
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string DefaultUserStartPreference { get; set; }
        public int? DefaultOrganizationUnitId { get; set; }
        public Guid? DefaultOrganizationUnitUuid { get; set; }
        public string DefaultOrganizationUnitName { get; set; }
        public bool IsGlobalAdmin { get; set; }
        public List<OrganizationRightDTO> OrganizationRights { get; set; }
        public string ObjectOwnerName { get; set; }
        public string ObjectOwnerLastName { get; set; }
        public DateTime? LastAdvisDate { get; set; }
        public DateTime LastChanged { get; set; }
        public int? LastChangedByUserId { get; set; }
        public bool? HasApiAccess { get; set; }

        public string FullName => Name + " " + LastName;
    }
}
