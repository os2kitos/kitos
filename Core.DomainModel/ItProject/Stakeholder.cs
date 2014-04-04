namespace Core.DomainModel.ItProject
{
    public class Stakeholder
    {
        public int Id { get; set; }
        public int ItProjectId { get; set; }

        public virtual ItProject ItProject { get; set; }
    }
}
