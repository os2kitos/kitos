namespace Core.DomainModel
{
    public class Config : IEntity<int>
    {
        public int Id { get; set; }
        public bool ShowItProjectModule { get; set; }
        public bool ShowItSystemModule { get; set; }
        public bool ShowItContractModule { get; set; }

        public int ItSupportModuleName_Id { get; set; }
        public int ItProjectModuleName_Id { get; set; }
        public int ItSystemModuleName_Id { get; set; }
        public int ItContractModuleName_Id { get; set; }

        public virtual ItSupportModuleName ItSupportModuleName { get; set; }
        public virtual ItProjectModuleName ItProjectModuleName { get; set; }
        public virtual ItSystemModuleName ItSystemModuleName { get; set; }
        public virtual ItContractModuleName ItContractModuleName { get; set; }
        public virtual Municipality Municipality { get; set; }
    }
}
