using Presentation.Web.Models.API.V2.Response;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.SharedProperties
{
    public interface IHasEntityCreator
    {
        public IdentityNamePairResponseDTO CreatedBy { get; set; }
    }
}
