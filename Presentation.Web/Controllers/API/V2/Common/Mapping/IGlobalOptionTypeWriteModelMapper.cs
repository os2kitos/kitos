

using Core.ApplicationServices.Model.GlobalOptions;
using Presentation.Web.Models.API.V2.Internal.Request;
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public interface IGlobalOptionTypeWriteModelMapper
    {
        GlobalRegularOptionCreateParameters ToGlobalRegularOptionCreateParameters(GlobalRegularOptionCreateRequestDTO dto);
        GlobalRegularOptionUpdateParameters ToGlobalRegularOptionUpdateParameters(GlobalRegularOptionUpdateRequestDTO dto);

        GlobalRoleOptionCreateParameters ToGlobalRoleOptionCreateParameters(GlobalRoleOptionCreateRequestDTO dto);
        GlobalRoleOptionUpdateParameters ToGlobalRoleOptionUpdateParameters(GlobalRoleOptionUpdateRequestDTO dto);
    }
}