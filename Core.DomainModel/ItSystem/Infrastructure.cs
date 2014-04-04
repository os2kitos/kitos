namespace Core.DomainModel.ItSystem
{
    public class Infrastructure
    {
        public int Id { get; set; }
        public int HostId { get; set; }
        public int SupplierId { get; set; }
        public int DepartmentId { get; set; }

        public virtual OrganizationUnit OrganizationUnit { get; set; }
        public virtual Host Host { get; set; }
        public virtual ItSystem ItSystem { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}
