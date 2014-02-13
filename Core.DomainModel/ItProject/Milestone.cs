namespace Core.DomainModel.ItProject
{
    public class Milestone
    {
        public int Id { get; set; }
        public int ProjectStatus_Id { get; set; }
        public virtual ProjectStatus ProjectStatus { get; set; }
    }
}
