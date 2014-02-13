namespace Core.DomainModel.ItSystem
{
    public partial class TaskSupport
    {
        public int Id { get; set; }
        public int ItSystem_Id { get; set; }
        public virtual ItSystem ItSystem { get; set; }
    }
}
