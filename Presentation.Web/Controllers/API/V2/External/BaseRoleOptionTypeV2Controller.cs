using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Core.DomainServices.Model.Options;
using Presentation.Web.Models.API.V2.Response.Options;

namespace Presentation.Web.Controllers.API.V2.External
{
    public abstract class BaseRoleOptionTypeV2Controller<TParent, TOption> : BaseOptionTypeV2Controller<TParent, TOption, RoleOptionResponseDTO, RoleOptionExtendedResponseDTO>
        where TOption : OptionEntity<TParent>, IRoleEntity
    {
        protected override RoleOptionResponseDTO ToDTO(OptionDescriptor<TOption> option)
        {
            return new RoleOptionResponseDTO(option.Option.Uuid, option.Option.Name, option.Option.HasWriteAccess, option.Description);
        }

        protected override RoleOptionExtendedResponseDTO ToExtendedDTO(OptionDescriptor<TOption> option, bool isAvailable)
        {
            return new RoleOptionExtendedResponseDTO(option.Option.Uuid, option.Option.Name, option.Option.HasWriteAccess, isAvailable, option.Description);
        }

        protected BaseRoleOptionTypeV2Controller(IOptionsApplicationService<TParent, TOption> optionApplicationService)
            : base(optionApplicationService)
        {

        }
    }
}