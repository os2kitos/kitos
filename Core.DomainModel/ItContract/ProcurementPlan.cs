namespace Core.DomainModel.ItContract
{
    public class ProcurementPlan
    {
        public int Half { get; set; }
        public int Year { get; set; }

        public virtual ItContract ItContract { get; set; }
    }
}