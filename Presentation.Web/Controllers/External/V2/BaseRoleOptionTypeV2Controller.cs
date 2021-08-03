using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Response.Options;

namespace Presentation.Web.Controllers.External.V2
{
    public abstract class BaseRoleOptionTypeV2Controller<TParent, TOption> : BaseOptionTypeV2Controller<TParent, TOption, RoleOptionResponseDTO, RoleOptionExtendedResponseDTO>
        where TOption : OptionEntity<TParent>, IRoleEntity
    {
        protected override RoleOptionResponseDTO ToDTO(TOption option)
        {
            return new(option.Uuid, option.Name, option.HasWriteAccess);
        }

        protected override RoleOptionExtendedResponseDTO ToExtendedDTO(TOption option, bool isAvailable)
        {
            return new(option.Uuid, option.Name, option.HasWriteAccess, isAvailable);
        }

        protected BaseRoleOptionTypeV2Controller(IOptionsApplicationService<TParent, TOption> optionApplicationService)
            : base(optionApplicationService)
        {

        }
    }
}