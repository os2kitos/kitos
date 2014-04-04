namespace Core.DomainModel.ItProject
{
    public class Milestone
    {
        public int Id { get; set; }
        public int ProjectStatusId { get; set; }
        public virtual ProjectStatus ProjectStatus { get; set; }
    }
}
