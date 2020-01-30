namespace Presentation.Web.Models.SystemRelations
{
    public class SystemRelationDTO
    {
        public int Id { get; set; }
        public NamedEntityDTO Source { get; set; }
        public NamedEntityDTO Destination { get; set; }
        public NamedEntityDTO Interface { get; set; }
        public NamedEntityDTO Contract { get; set; }
        public NamedEntityDTO FrequencyType { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
    }
}