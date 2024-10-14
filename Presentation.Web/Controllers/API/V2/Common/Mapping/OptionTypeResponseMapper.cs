using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Response.Options;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping;

public class OptionTypeResponseMapper : IOptionTypeResponseMapper
{
    public IEnumerable<RegularOptionResponseDTO> ToRegularOptionDTOs<TReference, TOption>(IEnumerable<TOption> options) where TOption : OptionEntity<TReference>
    {
        return options.Select(ToRegularOptionDTO<TReference, TOption>);
    }

    public RegularOptionResponseDTO ToRegularOptionDTO<TReference, TOption>(TOption option)
        where TOption : OptionEntity<TReference>
    {
        return new(option.Uuid, option.Name, option.Description);
    }
}