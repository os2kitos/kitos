using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions;

namespace Presentation.Web.Controllers.API.V2.Internal.Mapping;

public class GlobalOptionTypeResponseMapper : IGlobalOptionTypeResponseMapper
{
    public IEnumerable<GlobalRegularOptionResponseDTO> ToGlobalRegularOptionDTOs<TOption, TReference>(IEnumerable<TOption> options) where TOption : OptionEntity<TReference>
    {
        return options.Select(ToGlobalRegularOptionDTO<TOption, TReference>);
    }

    public GlobalRoleOptionResponseDTO ToGlobalRoleOptionDTO<TOption, TReference>(TOption option) where TOption : OptionEntity<TReference>, IRoleEntity
    {
        return new()
        {
            Description = option.Description,
            Name = option.Name,
            Uuid = option.Uuid,
            IsObligatory = option.IsObligatory,
            IsEnabled = option.IsEnabled,
            Priority = option.Priority,
            WriteAccess = option.HasWriteAccess
        };
    }

    public IEnumerable<GlobalRoleOptionResponseDTO> ToGlobalRoleOptionDTOs<TOption, TReference>(IEnumerable<TOption> options) where TOption : OptionEntity<TReference>, IRoleEntity
    {
        return options.Select(ToGlobalRoleOptionDTO<TOption, TReference>);
    }

    public GlobalRegularOptionResponseDTO ToGlobalRegularOptionDTO<TOption, TReference>(TOption option) where TOption : OptionEntity<TReference>
    {
        return new()
        {
            Description = option.Description,
            IsObligatory = option.IsObligatory,
            Name = option.Name,
            IsEnabled = option.IsEnabled,
            Uuid = option.Uuid,
            Priority = option.Priority
        };
    }
}