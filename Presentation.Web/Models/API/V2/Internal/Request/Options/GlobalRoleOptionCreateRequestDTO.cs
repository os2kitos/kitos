

namespace Presentation.Web.Models.API.V2.Internal.Request.Options
{
    public class GlobalRoleOptionCreateRequestDTO: GlobalRegularOptionCreateRequestDTO
    {
        public bool WriteAccess { get; set; }
    }
}