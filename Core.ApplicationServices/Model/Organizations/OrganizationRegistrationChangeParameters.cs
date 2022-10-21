using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Organizations
{
    public class OrganizationRegistrationChangeParameters
    {
        public int Id { get; set; }
        public OrganizationRegistrationType Type { get; set; }

        public OrganizationRegistrationChangeParameters(int id, OrganizationRegistrationType type)
        {
            Id = id;
            Type = type;
        }
    }
}
