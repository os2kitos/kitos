using Core.ApplicationServices.Model.LocalOptions;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public class LocalOptionTypeWriteModelMapper : WriteModelMapperBase, ILocalOptionTypeWriteModelMapper
    {
        public LocalOptionCreateParameters ToLocalOptionCreateParameters(LocalRegularOptionCreateRequestDTO dto)
        {
            return new()
            {
                OptionId = dto.OptionId,
            };
        }

        public LocalOptionTypeWriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest)
        {
        }
    }
}