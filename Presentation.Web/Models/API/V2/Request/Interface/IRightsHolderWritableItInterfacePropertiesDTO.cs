using System;

namespace Presentation.Web.Models.API.V2.Request.Interface
{
    public interface IRightsHolderWritableItInterfacePropertiesDTO : ICommonItInterfaceRequestPropertiesDTO
    {
        public Guid ExposedBySystemUuid { get; set; }
    }
}
