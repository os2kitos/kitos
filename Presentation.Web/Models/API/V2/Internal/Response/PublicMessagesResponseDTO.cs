using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Internal.Response
{
    public class PublicMessagesResponseDTO
    {
        [Required(AllowEmptyStrings = true)]
        public string About { get; set; }
        [Required(AllowEmptyStrings = true)]
        public string Guides { get; set; }
        [Required(AllowEmptyStrings = true)]
        public string StatusMessages { get; set; }
        [Required(AllowEmptyStrings = true)]
        public string Misc { get; set; }
        [Required(AllowEmptyStrings = true)]
        public string ContactInfo { get; set; }
    }
}