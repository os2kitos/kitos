using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping;

public class LocalOptionTypeResponseMapper : ILocalOptionTypeResponseMapper
{
    public IEnumerable<LocalRegularOptionResponseDTO> ToLocalRegularOptionDTOs<TReference, TOption>(IEnumerable<TOption> options) where TOption : OptionEntity<TReference>
    {
        return options.Select(ToLocalRegularOptionDTO<TReference, TOption>);
    }

    public LocalRegularOptionResponseDTO ToLocalRegularOptionDTO<TReference, TOption>(TOption option) where TOption : OptionEntity<TReference>
    {
        return new(option.Uuid, option.Name, option.Description, option.IsLocallyAvailable, option.IsObligatory);
    }
}