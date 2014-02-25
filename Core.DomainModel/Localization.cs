namespace Core.DomainModel
{
    public class Localization : IEntity<string>
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public int Municipality_Id { get; set; }

        public virtual Municipality Municipality { get; set; }
    }
}
