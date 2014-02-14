namespace Core.DomainModel.ItProject
{
    public class Resource
    {
        public int Id { get; set; }
        public int ItProject_Id { get; set; }

        public virtual ItProject ItProject { get; set; }
    }
}
