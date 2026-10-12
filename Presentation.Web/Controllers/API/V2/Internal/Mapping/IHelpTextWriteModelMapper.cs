using Core.ApplicationServices.Model.HelpTexts;
using Presentation.Web.Models.API.V2.Internal.Request;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.Internal.Mapping
{
    public interface IHelpTextWriteModelMapper
    {
        HelpTextCreateParameters ToCreateParameters(HelpTextCreateRequestDTO dto);
        HelpTextUpdateParameters ToUpdateParameters(HelpTextUpdateRequestDTO dto);
    }
}
