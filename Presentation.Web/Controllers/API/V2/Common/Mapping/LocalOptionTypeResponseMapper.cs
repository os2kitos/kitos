using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response;
using Presentation.Web.Models.API.V2.Internal.Response.LocalOptions;

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

    public IEnumerable<LocalRoleOptionResponseDTO> ToLocalRoleOptionDTOs<TReference, TOption>(IEnumerable<TOption> options) where TOption : OptionEntity<TReference>, IRoleEntity
    {
        return options.Select(ToLocalRoleOptionDTO<TReference, TOption>);
    }

    public LocalRoleOptionResponseDTO ToLocalRoleOptionDTO<TReference, TOption>(TOption option) where TOption : OptionEntity<TReference>, IRoleEntity
    {
        return new(option.Uuid, option.Name, option.Description, option.IsLocallyAvailable, option.IsObligatory, option.HasWriteAccess);
    }
}