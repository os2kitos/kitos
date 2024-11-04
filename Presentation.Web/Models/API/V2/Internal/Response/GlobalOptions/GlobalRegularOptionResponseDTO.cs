using System;
using Presentation.Web.Models.API.V2.Response.Options;

namespace Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions
{
    public class GlobalRegularOptionResponseDTO: RegularOptionResponseDTO
    {
        public bool IsEnabled { get; set; }
        public bool IsObligatory { get; set; }
        public int Priority { get; set; }
    }

}