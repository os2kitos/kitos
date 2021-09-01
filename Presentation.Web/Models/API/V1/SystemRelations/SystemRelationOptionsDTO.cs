namespace Presentation.Web.Models.API.V1.SystemRelations
{
    public class SystemRelationOptionsDTO
    {
        public NamedEntityDTO[] AvailableInterfaces { get; set; }
        public NamedEntityDTO[] AvailableContracts { get; set; }
        public NamedEntityDTO[] AvailableFrequencyTypes { get; set; }
    }
}