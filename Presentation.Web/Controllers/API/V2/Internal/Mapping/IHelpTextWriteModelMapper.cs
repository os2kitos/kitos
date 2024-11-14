using Core.ApplicationServices.Model.HelpTexts;
using Presentation.Web.Models.API.V2.Internal.Request;

namespace Presentation.Web.Controllers.API.V2.Internal.Mapping
{
    public interface IHelpTextWriteModelMapper
    {
        HelpTextCreateParameters ToCreateParameters(HelpTextCreateRequestDTO dto);
        HelpTextUpdateParameters ToUpdateParameters(HelpTextUpdateRequestDTO dto);
    }
}
