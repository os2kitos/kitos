
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Presentation.Web.Models.API.V2.Internal.Request
{
    public class GlobalRoleOptionUpdateRequestDTO: GlobalRegularOptionUpdateRequestDTO
    {
        public bool WriteAccess { get; set; }
    }
}