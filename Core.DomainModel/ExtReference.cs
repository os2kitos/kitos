namespace Core.DomainModel
{
    public class ExtReference
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public int ItProjectId { get; set; }
        public int ExtReferenceTypeId { get; set; }
        public int ItSystemId { get; set; }

        public virtual ExtReferenceType ExtReferenceType { get; set; }
        public virtual ItProject.ItProject ItProject { get; set; }
        public virtual ItSystem.ItSystem ItSystem { get; set; }
    }
}
