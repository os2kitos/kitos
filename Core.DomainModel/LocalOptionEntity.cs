using System;
using Core.Abstractions.Types;

namespace Core.DomainModel
{
    public abstract class LocalOptionEntity<TOptionType> : Entity, IOwnedByOrganization
    {
        public string Description { get; set; }

        public virtual Organization.Organization Organization { get; set; }

        public int OrganizationId { get; set; }

        [Obsolete("This will never be used")]  
        public virtual OptionEntity<TOptionType> Option { get; set; }

        public int OptionId { get; set; }

        public bool IsActive { get; set; }

        public void SetupNewLocalOption(int organizationId, int optionId)
        {
            OrganizationId = organizationId;
            OptionId = optionId;
            Activate();
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void UpdateDescription(Maybe<string> description)
        {
            if (description.HasValue) Description = description.Value;
        }
    }
}
