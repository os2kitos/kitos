namespace Core.DomainModel.ItContract.Read
{
    public class ItContractOverviewReadModelSystemRelation : IHasId

    {
        public int Id { get; set; }
        public int RelationId { get; set; }
        public int FromSystemUsageId { get; set; }
        public int ToSystemUsageId { get; set; }
        public int ParentId { get; set; }
        public virtual ItContractOverviewReadModel Parent { get; set; }
    }
}
