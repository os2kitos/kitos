

using Core.ApplicationServices.Model.GlobalOptions;
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public interface IGlobalOptionTypeWriteModelMapper
    {
        GlobalOptionCreateParameters ToGlobalOptionCreateParameters(GlobalOptionCreateRequestDTO dto);

    }
}