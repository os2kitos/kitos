namespace Presentation.Web.Models.SystemRelations
{
    public class SystemRelationOptionsDTO
    {
        public NamedEntityDTO[] AvailableInterfaces { get; set; }
        public NamedEntityDTO[] AvailableContracts { get; set; }
        public NamedEntityDTO[] AvailableFrequencyTypes { get; set; }
    }
}