using System.ComponentModel.DataAnnotations;
using Core.DomainModel.PublicMessage;
using Presentation.Web.Models.API.V2.Internal.Response;

namespace Presentation.Web.Models.API.V2.Internal.Request
{
    public class PublicMessageRequestDTO
    {
        [MaxLength(PublicMessage.DefaultTitleMaxLength)]
        public string Title { get; set; }
        public string LongDescription { get; set; }
        [MaxLength(PublicMessage.DefaultShortDescriptionMaxLength)]
        public string ShortDescription { get; set; }

        public PublicMessageStatusChoice? Status { get; set; }
        public string Link { get; set; }
    }
}