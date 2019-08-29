using Core.DomainModel.Advice;

namespace Presentation.Web.Models
{
    public class AdviceUserRelationDTO
    {
        public string Name { get; set; }
        public RecieverType RecieverType { get; set; }
        public RecieverType RecpientType { get; set; }
        public int? AdviceId { get; set; }
    }
}