using Presentation.Web.Models.API.V2.Response.Options;
using System;

namespace Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions
{
    public class GlobalRegularOptionResponseDTO: RegularOptionResponseDTO
    {
        public bool IsEnabled { get; set; }
        public bool IsObligatory { get; set; }
        public int Priority { get; set; }

        public GlobalRegularOptionResponseDTO(Guid uuid, string name, string description, bool isEnabled, bool isObligatory, int priority) : base(uuid, name,
            description)
        {
            IsEnabled = isEnabled;
            IsObligatory = isObligatory;
            Priority = priority;
        }
    }

}