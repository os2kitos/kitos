namespace Core.DomainModel.ItContract
{
    public class ProcurementPlan : IEntity<int>
    {
        public int Id { get; set; }
        public int Half { get; set; }
        public int Year { get; set; }

        public virtual ItContract ItContract { get; set; }
    }
}