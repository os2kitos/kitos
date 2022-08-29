namespace Core.DomainModel.ItContract.Read
{
    public class ItContractOverviewReadModel : IOwnedByOrganization, IReadModel<ItContract>, IHasName
    {
        public int OrganizationId { get; set; }
        public virtual Organization.Organization Organization { get; set; }
        public int Id { get; set; }
        public int SourceEntityId { get; set; }
        public virtual ItContract SourceEntity { get; set; }
        public string Name { get; set; }
    }
}
