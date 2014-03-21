namespace Core.DomainModel.ItProject
{
    public class OrgTab
    {
        public int Id { get; set; }

        public virtual ItProject ItProject { get; set; }
    }
}
