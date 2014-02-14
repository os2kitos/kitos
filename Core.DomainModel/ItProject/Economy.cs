namespace Core.DomainModel.ItProject
{
    public class Economy
    {
        public int Id { get; set; }
        public int ItProject_Id { get; set; }

        public virtual ItProject ItProject { get; set; }
    }
}
