namespace Core.DomainModel.ItSystem
{
    public class BasicData
    {
        public int Id { get; set; }
        public int ItSystemId { get; set; }

        public virtual ItSystem ItSystem { get; set; }
    }
}
