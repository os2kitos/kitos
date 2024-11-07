
using System;

namespace Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions
{
    public class GlobalRoleOptionResponseDTO: GlobalRegularOptionResponseDTO
    {
        public bool WriteAccess { get; set; }

        public GlobalRoleOptionResponseDTO(Guid uuid, string name, string description, bool isEnabled, bool isObligatory, int priority, bool writeAccess) : base(uuid, name, description, isEnabled, isObligatory, priority)
        {
            WriteAccess = writeAccess;
        }
    }
}