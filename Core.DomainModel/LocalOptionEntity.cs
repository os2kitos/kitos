using System;

namespace Core.DomainModel
{
    public abstract class LocalOptionEntity<OptionType> : Entity, IOwnedByOrganization
    {
        public string Description { get; set; }

        public virtual Organization.Organization Organization { get; set; }

        public int OrganizationId { get; set; }

        [Obsolete("This will never be used")]  
        public virtual OptionEntity<OptionType> Option { get; set; }

        public int OptionId { get; set; }

        public bool IsActive { get; set; }

    }
}
