namespace Core.DomainModel.ItSystem
{
    public class TaskSupport
    {
        public int Id { get; set; }
        public int ItSystem_Id { get; set; }

        public virtual ItSystem ItSystem { get; set; }
    }
}
