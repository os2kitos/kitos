namespace Presentation.Web.Models.SystemRelations
{
    public class CreateSystemRelationDTO
    {
        public int SourceUsageId { get; set; }
        public int TargetUsageId { get; set; }
        public string Description { get; set; }
        public int? InterfaceId { get; set; }
        public int? FrequencyTypeId { get; set; }
        public int? ContractId { get; set; }
        public string Reference { get; set; }
    }
}