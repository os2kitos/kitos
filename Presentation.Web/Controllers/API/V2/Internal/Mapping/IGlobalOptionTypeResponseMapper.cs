
using System.Collections.Generic;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions;

namespace Presentation.Web.Controllers.API.V2.Internal.Mapping
{
    public interface IGlobalOptionTypeResponseMapper
    {

        public IEnumerable<GlobalRegularOptionResponseDTO> ToGlobalRegularOptionDTOs<TOption, TReference>(IEnumerable<TOption> options)
            where TOption : OptionEntity<TReference>;

    }
}