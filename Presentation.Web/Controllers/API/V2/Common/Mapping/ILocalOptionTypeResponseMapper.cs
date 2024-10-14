using System.Collections.Generic;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public interface ILocalOptionTypeResponseMapper
    {
        public IEnumerable<LocalRegularOptionResponseDTO> ToLocalRegularOptionDTOs<TReference, TOption>(IEnumerable<TOption> options)
            where TOption : OptionEntity<TReference>;

        public LocalRegularOptionResponseDTO OptionEntityToLocalRegularOptionDTO<TReference, TOption>(TOption option)
            where TOption : OptionEntity<TReference>;

        public LocalRegularOptionResponseDTO LocalOptionEntityToLocalRegularOptionDTO<TReference, TOption, TLocalOption>(TLocalOption option)
            where TOption: OptionEntity<TReference>
            where TLocalOption : LocalOptionEntity<TOption>;

    }
}