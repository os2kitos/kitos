using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Internal.Response
{
    public class PublicTextsResponseDTO
    {
        [Required(AllowEmptyStrings = true)]
        public string Introduction { get; set; }
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