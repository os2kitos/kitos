namespace Core.DomainModel.ItProject
{
    public class Risk
    {
        public int Id { get; set; }
        public int ItProjectId { get; set; }

        public virtual ItProject ItProject { get; set; }
    }
}
