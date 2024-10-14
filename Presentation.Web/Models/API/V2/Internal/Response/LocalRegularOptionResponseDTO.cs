using Presentation.Web.Models.API.V2.Response.Options;
using System;

namespace Presentation.Web.Models.API.V2.Internal.Response
{
    public class LocalRegularOptionResponseDTO: RegularOptionResponseDTO
    {
        public LocalRegularOptionResponseDTO(Guid uuid, string name, string description, bool isActive, bool isObligatory) : base(uuid, name, description)
        {
            IsActive = isActive;
            IsObligatory = isObligatory;
        }
        public bool IsActive { get; set; }
        public bool IsObligatory { get; set; }
    }
}