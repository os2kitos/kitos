using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Core.DomainServices.Model.Options;
using Microsoft.AspNetCore.Mvc;
using Presentation.Web.Extensions;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Presentation.Web.Controllers.API.V2.External
{
    public abstract class BaseRoleOptionTypeV2Controller<TParent, TOption> : BaseOptionTypeV2Controller<TParent, TOption, RoleOptionResponseDTO, RoleOptionExtendedResponseDTO>
        where TOption : OptionEntity<TParent>, IRoleEntity
    {
        protected override IActionResult GetAvailableOptions(Guid organizationUuid, [FromQuery] UnboundedPaginationQuery? pagination = null)
        {
            return _roleOptionApplicationService
                .GetOptionTypes(organizationUuid)
                .Select(x => x.Page(pagination))
                .Select(ToDTOs)
                .Match(Ok, FromOperationError);
        }

        private List<RoleOptionResponseDTO> ToDTOs(IEnumerable<OptionDescriptor<TOption>> options)
        {
            return options.Select(ToDTO).ToList();
        }

        protected override RoleOptionResponseDTO ToDTO(OptionDescriptor<TOption> option)
        {
            return new RoleOptionResponseDTO(option.Option.Uuid, option.Option.Name, option.Option.HasWriteAccess, option.Description, option.Option.RoleIsExternallyUsed, option.Option.RoleExternallyUsedDescription);
        }

        protected override RoleOptionExtendedResponseDTO ToExtendedDTO(OptionDescriptor<TOption> option, bool isAvailable)
        {
            return new RoleOptionExtendedResponseDTO(option.Option.Uuid, option.Option.Name, option.Option.HasWriteAccess, isAvailable, option.Description, option.Option.RoleIsExternallyUsed, option.Option.RoleExternallyUsedDescription);
        }

        private readonly IRoleOptionsApplicationService<TParent, TOption> _roleOptionApplicationService;
        protected BaseRoleOptionTypeV2Controller(IOptionsApplicationService<TParent, TOption> optionApplicationService, IRoleOptionsApplicationService<TParent, TOption> roleOptionApplicationService)
            : base(optionApplicationService)
        {
            _roleOptionApplicationService = roleOptionApplicationService;
        }
    }
}
