namespace Presentation.Web.Models.SystemRelations
{
    public class SystemRelationDTO
    {
        public int Id { get; set; }
        public NamedEntityDTO FromUsage { get; set; }
        public NamedEntityDTO ToUsage { get; set; }
        public NamedEntityDTO Interface { get; set; }
        public NamedEntityDTO Contract { get; set; }
        public NamedEntityDTO FrequencyType { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }

        public SystemRelationDTO() {}

        public SystemRelationDTO(int id, NamedEntityDTO fromUsage, NamedEntityDTO toUsage, NamedEntityDTO @interface, 
            NamedEntityDTO contract, NamedEntityDTO frequencyType, string description, string reference)
        {
            Id = id;
            FromUsage = fromUsage;
            ToUsage = toUsage;
            Interface = @interface;
            Contract = contract;
            FrequencyType = frequencyType;
            Description = description;
            Reference = reference;
        }
    }
}