using System;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage
{
    public class ItSystemUsageSystemContextResponseDTO : IdentityNamePairWithDeactivatedStatusDTO
    {
        protected ItSystemUsageSystemContextResponseDTO()
        {
        }

        public ItSystemUsageSystemContextResponseDTO(Guid uuid, string name, bool deactivated) 
            : base(uuid, name, deactivated)
        {
        }
    }
}