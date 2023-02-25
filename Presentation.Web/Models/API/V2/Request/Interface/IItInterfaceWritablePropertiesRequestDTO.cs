using System;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Request.Interface
{
    public interface IItInterfaceWritablePropertiesRequestDTO : ICommonItInterfaceRequestPropertiesDTO
    {
        public Guid? ExposedBySystemUuid { get; set; }
        public bool Disabled { get; set; }
        public RegistrationScopeChoice Scope { get; set; }
        public Guid? ItInterfaceTypeUuid { get; set; }
        public IEnumerable<ItInterfaceDataRequestDTO> Data { get; set; }
        public string Note { get; set; }
    }
}
