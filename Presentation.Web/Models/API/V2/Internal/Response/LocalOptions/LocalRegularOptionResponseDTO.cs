using Presentation.Web.Models.API.V2.Response.Options;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V2.Internal.Response
{
    public class LocalRegularOptionResponseDTO: RegularOptionResponseDTO
    {
        [SetsRequiredMembers]
        public LocalRegularOptionResponseDTO(Guid uuid, string name, string description, bool isLocallyAvailable, bool isObligatory) : base(uuid, name, description)
        {
            IsLocallyAvailable = isLocallyAvailable;
            IsObligatory = isObligatory;
            IsActive = IsObligatory || IsLocallyAvailable;
        }
        public bool IsActive { get; set; }
        public bool IsObligatory { get; set; }
        public bool IsLocallyAvailable { get; set; }
    }
}