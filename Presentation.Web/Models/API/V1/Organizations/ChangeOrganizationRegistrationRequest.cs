using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class ChangeOrganizationRegistrationRequest
    {
        public int Id { get; set; }
        public OrganizationRegistrationOption Type { get; set; }

        public ChangeOrganizationRegistrationRequest(int id, OrganizationRegistrationOption type)
        {
            Id = id;
            Type = type;
        }
    }
}