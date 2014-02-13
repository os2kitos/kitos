namespace Core.DomainModel
{
    public partial class KLE
    {
        public int Id { get; set; }
        public int ItProject_Id { get; set; }
        public int ItSystem_Id { get; set; }
        public virtual ItProject.ItProject ItProject { get; set; }
        public virtual ItSystem.ItSystem ItSystem { get; set; }
    }
}
