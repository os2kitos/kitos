namespace Core.DomainModel.ItSystem
{
    public class Infrastructure
    {
        public int Id { get; set; }
        public int Host_Id { get; set; }
        public int Supplier_Id { get; set; }
        public int Department_Id { get; set; }

        public virtual Department Department { get; set; }
        public virtual Host Host { get; set; }
        public virtual ItSystem ItSystem { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}
