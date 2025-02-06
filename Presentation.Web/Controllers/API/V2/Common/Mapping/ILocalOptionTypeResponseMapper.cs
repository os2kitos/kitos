using System.Collections.Generic;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response;
using Presentation.Web.Models.API.V2.Internal.Response.LocalOptions;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public interface ILocalOptionTypeResponseMapper
    {
        public IEnumerable<LocalRegularOptionResponseDTO> ToLocalRegularOptionDTOs<TReference, TOption>(IEnumerable<TOption> options)
            where TOption : OptionEntity<TReference>;

        public LocalRegularOptionResponseDTO ToLocalRegularOptionDTO<TReference, TOption>(TOption option)
            where TOption : OptionEntity<TReference>;

        public IEnumerable<LocalRoleOptionResponseDTO> ToLocalRoleOptionDTOs<TReference, TOption>(IEnumerable<TOption> options)
            where TOption : OptionEntity<TReference>, IRoleEntity;

        public LocalRoleOptionResponseDTO ToLocalRoleOptionDTO<TReference, TOption>(TOption option)
            where TOption : OptionEntity<TReference>, IRoleEntity;
    }
}