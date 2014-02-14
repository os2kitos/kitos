namespace Core.DomainModel.ItSystem
{
    public class Component
    {
        public int Id { get; set; }
        public int ItSystem_Id { get; set; }

        public virtual ItSystem ItSystem { get; set; }
    }
}
