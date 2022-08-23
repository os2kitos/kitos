using Core.DomainModel.Advice;

namespace Presentation.Web.Models.API.V1
{
    public class AdviceUserRelationDTO
    {
        public string Email { get; set; }
        public RecieverType RecieverType { get; set; }
        public RecipientType RecpientType { get; set; }
        public int? AdviceId { get; set; }
        public int? ItContractRoleId { get; set; }
        public int? ItProjectRoleId { get; set; }
        public int? ItSystemRoleId { get; set; }
        public int? DataProcessingRegistrationRoleId { get; set; }
    }
}