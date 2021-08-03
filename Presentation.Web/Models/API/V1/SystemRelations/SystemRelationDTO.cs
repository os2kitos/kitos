using System;

namespace Presentation.Web.Models.API.V1.SystemRelations
{
    public class SystemRelationDTO
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; }
        public NamedEntityWithEnabledStatusDTO FromUsage { get; set; }
        public NamedEntityWithEnabledStatusDTO ToUsage { get; set; }
        public NamedEntityDTO Interface { get; set; }
        public NamedEntityDTO Contract { get; set; }
        public NamedEntityDTO FrequencyType { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }

        public SystemRelationDTO() {}

        public SystemRelationDTO(
            int id,
            Guid uuid,
            NamedEntityWithEnabledStatusDTO fromUsage,
            NamedEntityWithEnabledStatusDTO toUsage, 
            NamedEntityDTO @interface, 
            NamedEntityDTO contract, 
            NamedEntityDTO frequencyType, 
            string description, 
            string reference)
        {
            Id = id;
            Uuid = uuid;
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