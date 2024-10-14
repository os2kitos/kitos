using Presentation.Web.Models.API.V2.Response.Options;
using System;

namespace Presentation.Web.Models.API.V2.Internal.Response
{
    public class LocalRegularOptionResponseDTO: RegularOptionResponseDTO
    {
        public LocalRegularOptionResponseDTO(Guid uuid, string name, string description, bool isActive) : base(uuid, name, description)
        {
            IsActive = isActive;
        }
        public bool IsActive { get; set; }
    }
}