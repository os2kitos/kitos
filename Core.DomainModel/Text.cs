namespace Core.DomainModel
{
    public class Text : IEntity<string>
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }
}