namespace Core.DomainModel
{
    public class ExternalLink : Entity
    {
        public ExternalLink()
        {
        }

        public string Name { get; set; }
        public string Url { get; set; }
    }
}
