using System;

namespace Presentation.Web.Models.API.V2.Internal.Response.LocalOptions
{
    public class LocalRoleOptionResponseDTO: LocalRegularOptionResponseDTO
    {
        public bool WriteAccess { get; set; }

        public LocalRoleOptionResponseDTO(Guid uuid, string name, string description, bool isLocallyAvailable, bool isObligatory, bool writeAccess) 
            : base(uuid, name, description, isLocallyAvailable, isObligatory)
        {
            WriteAccess = writeAccess;
        }
    }
}