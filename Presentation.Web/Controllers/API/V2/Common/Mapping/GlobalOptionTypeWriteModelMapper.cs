using Core.Abstractions.Extensions;
using Core.ApplicationServices.Model.GlobalOptions;
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
            IsEnabled = GetOptionalValueChange(() => rule.MustUpdate(x => x.IsEnabled), dto.IsEnabled.FromNullable()),
            Name = GetOptionalValueChange(() => rule.MustUpdate(x => x.Name), dto.Name.FromNullable()),
            IsObligatory = GetOptionalValueChange(() => rule.MustUpdate(x => x.IsObligatory), dto.IsObligatory.FromNullable()),
            Description = GetOptionalValueChange(() => rule.MustUpdate(x => x.Description), dto.Description.FromNullable()),
            Priority = GetOptionalValueChange(() => rule.MustUpdate(x => x.Priority), dto.Priority.FromNullable()),
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
            IsEnabled = GetOptionalValueChange(() => rule.MustUpdate(x => x.IsEnabled), dto.IsEnabled.FromNullable()),
            Name = GetOptionalValueChange(() => rule.MustUpdate(x => x.Name), dto.Name.FromNullable()),
            IsObligatory = GetOptionalValueChange(() => rule.MustUpdate(x => x.IsObligatory), dto.IsObligatory.FromNullable()),
            Description = GetOptionalValueChange(() => rule.MustUpdate(x => x.Description), dto.Description.FromNullable()),
            WriteAccess = GetOptionalValueChange(() => rule.MustUpdate(x => x.WriteAccess), dto.WriteAccess.FromNullable()),
            Priority = GetOptionalValueChange(() => rule.MustUpdate(x => x.Priority), dto.Priority.FromNullable()),
        };
    }

    public GlobalOptionTypeWriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest)
    {
    }
}