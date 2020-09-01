namespace Core.DomainModel.GDPR
{
    /// <summary>
    /// A READ optimized perspective of the data processing agreement model
    /// </summary>
    public class DataProcessingAgreementReadModel : IOwnedByOrganization, IReadModel<DataProcessingAgreement>
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public int OrganizationId { get; set; }

        public virtual Organization.Organization Organization { get; set; }

        public int SourceEntityId { get; set; }
        
        public virtual DataProcessingAgreement SourceEntity { get; set; } //TODO: Prevent expand on sourceentity!!
    }
}
