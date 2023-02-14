using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Core.DomainServices.Model.Options;
using Presentation.Web.Models.API.V2.Response.Options;

namespace Presentation.Web.Controllers.API.V2.External
{
    public abstract class BaseRegularOptionTypeV2Controller<TParent,TOption> 
        : BaseOptionTypeV2Controller<TParent,TOption, RegularOptionResponseDTO, RegularOptionExtendedResponseDTO> where TOption : OptionEntity<TParent>
    {
        protected override RegularOptionResponseDTO ToDTO(OptionDescriptor<TOption> option)
        {
            return new(option.Option.Uuid, option.Option.Name,option.Description);
        }

        protected override RegularOptionExtendedResponseDTO ToExtendedDTO(OptionDescriptor<TOption> option, bool isAvailable)
        {
            return new(option.Option.Uuid, option.Option.Name, isAvailable, option.Description);
        }

        protected BaseRegularOptionTypeV2Controller(IOptionsApplicationService<TParent, TOption> optionApplicationService) : base(optionApplicationService)
        {

        }
    }
}