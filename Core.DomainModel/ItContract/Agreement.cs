namespace Core.DomainModel.ItContract
{
    public class Agreement
    {
        public int Id { get; set; }

        public virtual ItContract ItContract { get; set; }
    }
}
