﻿using System.Collections.Generic;
// ReSharper disable VirtualMemberCallInConstructor

namespace Core.DomainModel.Organization
{
    public enum OrganizationTypeKeys
    {
        Kommune = 1,
        Interessefællesskab,
        Virksomhed,
        AndenOffentligMyndighed
    }
    public class OrganizationType
    {
        public OrganizationType()
        {
            Organizations = new HashSet<Organization>();
        }

        public OrganizationTypeKeys Id { get; set; }
        public string Name { get; set; }
        public OrganizationCategory Category { get; set; }
        public virtual ICollection<Organization> Organizations { get; set; }
    }
}
