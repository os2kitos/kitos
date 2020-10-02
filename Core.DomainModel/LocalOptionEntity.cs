namespace Core.DomainModel
{
    public abstract class LocalOptionEntity<OptionType> : Entity, IOwnedByOrganization
    {
        public string Description { get; set; }

        public virtual Organization.Organization Organization { get; set; }

        public int OrganizationId { get; set; }

        public virtual OptionEntity<OptionType> Option { get; set; }

        public int OptionId { get; set; }

        public bool IsActive { get; set; }

        public string GetActiveDescription()
        {
            return string.IsNullOrWhiteSpace(Description) ? Option.Description : Description;
        }
    }
}
