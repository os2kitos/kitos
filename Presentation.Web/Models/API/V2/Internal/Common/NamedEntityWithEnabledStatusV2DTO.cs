using System;

namespace Presentation.Web.Models.API.V2.Internal.Common
{
    public class NamedEntityWithEnabledStatusV2DTO : NamedEntityV2DTO
    {
        public bool Disabled { get; set; }

        public NamedEntityWithEnabledStatusV2DTO(int id, Guid uuid, string name, bool disabled)
            : base(id, uuid, name)
        {
            Disabled = disabled;
        }
    }
}