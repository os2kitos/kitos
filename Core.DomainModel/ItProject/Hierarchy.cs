namespace Core.DomainModel.ItProject
{
    public class Hierarchy
    {
        public int Id { get; set; }

        public virtual ItProject ItProject { get; set; }
        public virtual ItProject ItProjectRef { get; set; }
        public virtual ItProject ItProgramRef { get; set; }
    }
}
