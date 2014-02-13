namespace Core.DomainModel.ItSystem
{
    public class UserAdministration
    {
        public int Id { get; set; }

        public virtual ItSystem ItSystem { get; set; }
    }
}
