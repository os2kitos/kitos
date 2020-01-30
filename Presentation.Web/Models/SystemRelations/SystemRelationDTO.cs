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

        public SystemRelationDTO() {}

        public SystemRelationDTO(int id, NamedEntityDTO source, NamedEntityDTO destination, NamedEntityDTO @interface, NamedEntityDTO contract, NamedEntityDTO frequencyType, string description, string reference)
        {
            Id = id;
            Source = source;
            Destination = destination;
            Interface = @interface;
            Contract = contract;
            FrequencyType = frequencyType;
            Description = description;
            Reference = reference;
        }
    }
}