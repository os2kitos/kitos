using System;

namespace Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions
{
    public class GlobalRegularOptionResponseDTO
    {
        public bool IsEnabled { get; set; }
        public string Name { get; set; }
        public bool IsObligatory { get; set; }
        public string Description { get; set; }
        public Guid Uuid { get; set; }

        public int Priority { get; set; }
    }

}