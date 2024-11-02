using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.GlobalOptions;
using Core.ApplicationServices.Model.Shared;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping;

public class GlobalOptionTypeWriteModelMapper : WriteModelMapperBase, IGlobalOptionTypeWriteModelMapper
{
    public GlobalOptionCreateParameters ToGlobalOptionCreateParameters(GlobalOptionCreateRequestDTO dto)
    {
        return new()
        {
            Description = dto.Description,
            IsObligatory = dto.IsObligatory,
            Name = dto.Name,
        };
    }

    public GlobalOptionUpdateParameters ToGlobalOptionUpdateParameters(GlobalOptionUpdateRequestDTO dto)
    {
        var rule = CreateChangeRule<GlobalOptionUpdateRequestDTO>(false);

        return new()
        {
            IsEnabled = rule.MustUpdate(x => x.IsEnabled)
                ? (dto.IsEnabled.FromNullable() ?? Maybe<bool>.None).AsChangedValue()
                : OptionalValueChange<Maybe<bool>>.None,

            Name = rule.MustUpdate(x => x.Name)
                ? (dto.Name.FromNullable() ?? Maybe<string>.None).AsChangedValue()
                : OptionalValueChange<Maybe<string>>.None,

            IsObligatory = rule.MustUpdate(x => x.IsObligatory)
                ? (dto.IsObligatory.FromNullable() ?? Maybe<bool>.None).AsChangedValue()
                : OptionalValueChange<Maybe<bool>>.None,

            Description = rule.MustUpdate(x => x.Description)
                ? (dto.Description.FromNullable() ?? Maybe<string>.None).AsChangedValue()
                : OptionalValueChange<Maybe<string>>.None
        };
    }

    public GlobalOptionTypeWriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest)
    {
    }
}