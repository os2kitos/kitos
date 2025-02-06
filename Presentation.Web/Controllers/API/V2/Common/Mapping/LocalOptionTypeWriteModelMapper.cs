using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.LocalOptions;
using Core.ApplicationServices.Model.Shared;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public class LocalOptionTypeWriteModelMapper : WriteModelMapperBase, ILocalOptionTypeWriteModelMapper
    {
        public LocalOptionCreateParameters ToLocalOptionCreateParameters(LocalOptionCreateRequestDTO dto)
        {
            return new()
            {
                OptionUuid = dto.OptionUuid,
            };
        }

        public LocalOptionUpdateParameters ToLocalOptionUpdateParameters(LocalRegularOptionUpdateRequestDTO dto)
        {
            var rule = CreateChangeRule<LocalRegularOptionUpdateRequestDTO>(false);

            return new()
            {
                Description = rule.MustUpdate(x => x.Description)
                    ? (dto.Description.FromNullable() ?? Maybe<string>.None).AsChangedValue()
                    : OptionalValueChange<Maybe<string>>.None,
            };
        }

        public LocalOptionTypeWriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest)
        {
        }
    }
}