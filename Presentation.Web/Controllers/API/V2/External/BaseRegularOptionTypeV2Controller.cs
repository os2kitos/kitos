using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Options;

namespace Presentation.Web.Controllers.API.V2.External
{
    public abstract class BaseRegularOptionTypeV2Controller<TParent,TOption> 
        : BaseOptionTypeV2Controller<TParent,TOption,IdentityNamePairResponseDTO,RegularOptionExtendedResponseDTO> where TOption : OptionEntity<TParent>
    {
        protected override IdentityNamePairResponseDTO ToDTO(TOption option)
        {
            return new(option.Uuid, option.Name);
        }

        protected override RegularOptionExtendedResponseDTO ToExtendedDTO(TOption option, bool isAvailable)
        {
            return new(option.Uuid, option.Name, isAvailable);
        }

        protected BaseRegularOptionTypeV2Controller(IOptionsApplicationService<TParent, TOption> optionApplicationService) : base(optionApplicationService)
        {

        }
    }
}