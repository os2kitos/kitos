namespace Core.DomainServices.Repositories.KLE
{
    public class KLEChange
    {
        public KLEChangeType ChangeType { get; set; }
        public string Type { get; set; }
        public string TaskKey { get; set; }
        public string UpdatedDescription { get; set; }
    }
}