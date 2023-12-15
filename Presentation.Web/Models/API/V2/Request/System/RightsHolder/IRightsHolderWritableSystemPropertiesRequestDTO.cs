using System;
using Presentation.Web.Models.API.V2.Request.System.Shared;

namespace Presentation.Web.Models.API.V2.Request.System.RightsHolder
{
    public interface IRightsHolderWritableSystemPropertiesRequestDTO : IItSystemWriteRequestCommonPropertiesDTO
    {
        public Guid? ExternalUuid { get; set; }
    }
}
