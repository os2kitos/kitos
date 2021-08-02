using Presentation.Web.Models.External.V2.Response;

namespace Presentation.Web.Models.External.V2.SharedProperties
{
    public interface IHasEntityCreator
    {
        public IdentityNamePairResponseDTO CreatedBy { get; set; }
    }
}
