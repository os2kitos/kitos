namespace Core.DomainModel
{
    public class ExtReference
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public int ItProject_Id { get; set; }
        public int ExtReferenceType_Id { get; set; }
        public int ItSystem_Id { get; set; }

        public virtual ExtReferenceType ExtReferenceType { get; set; }
        public virtual ItProject.ItProject ItProject { get; set; }
        public virtual ItSystem.ItSystem ItSystem { get; set; }
    }
}
