namespace Core.DomainModel
{
    public class Configuration : IEntity<string>
    {
        public string Id { get; set; }
        public bool IsSelected { get; set; }
        public int Municipality_Id { get; set; }

        public virtual Municipality Municipality { get; set; }
    }
}
