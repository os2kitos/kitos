namespace Core.DomainModel.ItSystem
{
    public class Wish
    {
        public int Id { get; set; }
        public int? Functionality_Id { get; set; }
        public int? Interface_Id { get; set; }

        public virtual Functionality Functionality { get; set; }
        public virtual Interface Interface { get; set; }
    }
}
