using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Core.DomainServices.Model.Options;
using Presentation.Web.Extensions;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Presentation.Web.Controllers.API.V2.External
{
    public abstract class BaseRegularOptionTypeV2Controller<TParent,TOption> 
        : BaseOptionTypeV2Controller<TParent,TOption, RegularOptionResponseDTO, RegularOptionExtendedResponseDTO> where TOption : OptionEntity<TParent>
    {
        protected override IActionResult GetAvailableOptions(Guid organizationUuid, [FromQuery] UnboundedPaginationQuery? pagination = null)
        {
            return _optionApplicationService
                .GetOptionTypes(organizationUuid)
                .Select(x => x.Page(pagination))
                .Select(ToDTOs)
                .Match(Ok, FromOperationError);
        }

        private List<RegularOptionResponseDTO> ToDTOs(IEnumerable<OptionDescriptor<TOption>> options)
        {
            return options.Select(ToDTO).ToList();
        }

        protected override RegularOptionResponseDTO ToDTO(OptionDescriptor<TOption> option)
        {
            return new(option.Option.Uuid, option.Option.Name,option.Description);
        }

        protected override RegularOptionExtendedResponseDTO ToExtendedDTO(OptionDescriptor<TOption> option, bool isAvailable)
        {
            return new(option.Option.Uuid, option.Option.Name, isAvailable, option.Description);
        }

        private readonly IOptionsApplicationService<TParent, TOption> _optionApplicationService;
        protected BaseRegularOptionTypeV2Controller(IOptionsApplicationService<TParent, TOption> optionApplicationService) : base(optionApplicationService)
        {
            _optionApplicationService = optionApplicationService;
        }
    }
}

