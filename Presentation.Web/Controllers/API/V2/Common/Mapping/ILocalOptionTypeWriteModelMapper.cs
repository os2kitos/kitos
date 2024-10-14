using Core.ApplicationServices.Model.LocalOptions;
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping;

public interface ILocalOptionTypeWriteModelMapper
{
    public LocalOptionCreateParameters ToLocalOptionCreateParameters(LocalRegularOptionCreateRequestDTO dto);
}