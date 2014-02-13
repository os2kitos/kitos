namespace Core.DomainModel
{
    public class ExternalReference
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public int ItProject_Id { get; set; }
        public int ExternalReferenceType_Id { get; set; }
        public int ItSystem_Id { get; set; }
        public virtual ExternalReferenceType ExternalReferenceType { get; set; }
        public virtual ItProject.ItProject ItProject { get; set; }
        public virtual ItSystem.ItSystem ItSystem { get; set; }
    }
}
