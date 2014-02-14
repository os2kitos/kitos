namespace Core.DomainModel.ItProject
{
    public class Organization
    {
        public int Id { get; set; }

        public virtual ItProject ItProject { get; set; }
    }
}
