using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions;

namespace Presentation.Web.Controllers.API.V2.Internal.Mapping;

public class GlobalOptionTypeResponseMapper : IGlobalOptionTypeResponseMapper
{
    public IEnumerable<GlobalRegularOptionResponseDTO> ToGlobalRegularOptionDTOs<TOption, TReference>(IEnumerable<TOption> options) where TOption : OptionEntity<TReference>
    {
        return options.Select(ToGlobalRegularOptionResponseDto<TOption, TReference>);
    }

    private GlobalRegularOptionResponseDTO ToGlobalRegularOptionResponseDto<TOption, TReference>(TOption option) where TOption : OptionEntity<TReference>
    {
        return new()
        {
            Description = option.Description,
            IsObligatory = option.IsObligatory,
            Name = option.Name,
            IsEnabled = option.IsEnabled,
            Uuid = option.Uuid,
        };
    }
}