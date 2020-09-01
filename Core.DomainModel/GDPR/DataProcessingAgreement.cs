namespace Core.DomainModel.GDPR
{
    public class DataProcessingAgreement : Entity, IHasName, IOwnedByOrganization

    {
        public string Name { get; set; }

        public int OrganizationId { get; set; }

        public virtual Organization.Organization Organization { get; set; }
    }
}
