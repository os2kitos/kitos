using System;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Internal.Response.ItSystem
{
    public class ItSystemSearchResponseDTO : IdentityNamePairWithDeactivatedStatusDTO
    {

        public ItSystemSearchResponseDTO()
        {
        }

        public ItSystemSearchResponseDTO(Guid uuid, string name, bool deactivated) 
            : base(uuid, name, deactivated)
        {
        }
    }
}