namespace Core.DomainModel
{
    public abstract class LocalOptionEntity<OptionType> : Entity, IOwnedByOrganization, IContextAware
    {
        public string Description { get; set; }

        public virtual Organization.Organization Organization { get; set; }

        public int OrganizationId { get; set; }

        public virtual OptionEntity<OptionType> Option { get; set; }

        public int OptionId { get; set; }

        public bool IsActive { get; set; }

        public bool IsInContext(int organizationId)
        {
            return OrganizationId == organizationId;
        }
    }
}
