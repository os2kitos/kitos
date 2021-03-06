﻿namespace Core.DomainModel
{
    public class ContactPerson : Entity
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public virtual int? OrganizationId { get; set; }
        public virtual Organization.Organization Organization { get; set; }
    }
}
