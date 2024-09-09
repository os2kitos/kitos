using Presentation.Web.Models.API.V2.Internal.Common;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit
{
    public class PaymentRegistrationResponseDTO
    {
        public IdentityNamePairResponseDTO ItContract { get; set; }
        public int ItContractId { get; set; }
        public IEnumerable<NamedEntityV2DTO> InternalPayments { get; set; } = new List<NamedEntityV2DTO>();
        public IEnumerable<NamedEntityV2DTO> ExternalPayments { get; set; } = new List<NamedEntityV2DTO>();
    }
}