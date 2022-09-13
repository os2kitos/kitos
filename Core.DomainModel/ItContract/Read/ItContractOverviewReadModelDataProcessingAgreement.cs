namespace Core.DomainModel.ItContract.Read
{
    public class ItContractOverviewReadModelDataProcessingAgreement : IHasId
    {
        public int Id { get; set; }
        public int DataProcessingRegistrationId { get; set; } //used for linking
        public string DataProcessingRegistrationName { get; set; } //used as name when linking
        public int ParentId { get; set; }
        public virtual ItContractOverviewReadModel Parent { get; set; }
    }
}
