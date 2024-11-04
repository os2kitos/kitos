
using System.Collections.Generic;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Internal.Response.GlobalOptions;

namespace Presentation.Web.Controllers.API.V2.Internal.Mapping
{
    public interface IGlobalOptionTypeResponseMapper
    {
        GlobalRegularOptionResponseDTO ToGlobalRegularOptionDTO<TOption, TReference>(TOption option)
            where TOption : OptionEntity<TReference>;

        public IEnumerable<GlobalRegularOptionResponseDTO> ToGlobalRegularOptionDTOs<TOption, TReference>(IEnumerable<TOption> options)
            where TOption : OptionEntity<TReference>;

        GlobalRoleOptionResponseDTO ToGlobalRoleOptionDTO<TOption, TReference>(TOption option)
            where TOption : OptionEntity<TReference>, IRoleEntity;

        IEnumerable<GlobalRoleOptionResponseDTO> ToGlobalRoleOptionDTOs<TOption, TReference>(IEnumerable<TOption> options)
            where TOption : OptionEntity<TReference>, IRoleEntity;

    }
}