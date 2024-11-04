using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.GlobalOptions;
using Core.ApplicationServices.Model.Shared;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Internal.Request;
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping;

public class GlobalOptionTypeWriteModelMapper : WriteModelMapperBase, IGlobalOptionTypeWriteModelMapper
{
    public GlobalRegularOptionCreateParameters ToGlobalRegularOptionCreateParameters(GlobalRegularOptionCreateRequestDTO dto)
    {
        return new()
        {
            Description = dto.Description,
            IsObligatory = dto.IsObligatory,
            Name = dto.Name,
        };
    }

    public GlobalRegularOptionUpdateParameters ToGlobalRegularOptionUpdateParameters(GlobalRegularOptionUpdateRequestDTO dto)
    {
        var rule = CreateChangeRule<GlobalRegularOptionUpdateRequestDTO>(false);

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

    public GlobalRoleOptionCreateParameters ToGlobalRoleOptionCreateParameters(GlobalRoleOptionCreateRequestDTO dto)
    {
        return new()
        {
            Description = dto.Description,
            IsObligatory = dto.IsObligatory,
            Name = dto.Name,
            WriteAccess = dto.WriteAccess
        };
    }

    public GlobalRoleOptionUpdateParameters ToGlobalRoleOptionUpdateParameters(GlobalRoleOptionUpdateRequestDTO dto)
    {
        var rule = CreateChangeRule<GlobalRoleOptionUpdateRequestDTO>(false);

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
                : OptionalValueChange<Maybe<string>>.None,

            WriteAccess = rule.MustUpdate(x => x.WriteAccess)
                ? (dto.WriteAccess.FromNullable() ?? Maybe<bool>.None).AsChangedValue()
                : OptionalValueChange<Maybe<bool>>.None,
        };
    }

    public GlobalOptionTypeWriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest)
    {
    }
}