namespace Core.DomainModel.ItSystem
{
    public class Wish
    {
        public int Id { get; set; }
        public int? FunctionalityId { get; set; }
        public int? InterfaceId { get; set; }

        public virtual Functionality Functionality { get; set; }
        public virtual Interface Interface { get; set; }
    }
}
