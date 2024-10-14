using System.Collections.Generic;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Response.Options;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public interface IOptionTypeResponseMapper
    {
        public IEnumerable<RegularOptionResponseDTO> ToRegularOptionDTOs<TReference, TOption>(IEnumerable<TOption> options)
            where TOption : OptionEntity<TReference>;
    }
}