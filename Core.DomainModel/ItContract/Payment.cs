namespace Core.DomainModel.ItContract
{
    public class Payment
    {
        public int Id { get; set; }

        public virtual ItContract ItContract { get; set; }
    }
}
