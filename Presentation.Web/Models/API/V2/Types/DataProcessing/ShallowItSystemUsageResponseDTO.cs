using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Types.DataProcessing
{
    public class ShallowItSystemUsageResponseDTO: IdentityNamePairResponseDTO
    {
        public bool Valid { get; set; }
    }
}
