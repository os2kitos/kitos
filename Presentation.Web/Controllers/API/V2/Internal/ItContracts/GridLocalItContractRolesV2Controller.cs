using System;
using System.Linq;
using System.Collections.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Response.ItContract;
using Core.ApplicationServices.OptionTypes;
using Microsoft.AspNetCore.Mvc;
using Core.DomainModel.ItContract;

using System.Net;
namespace Presentation.Web.Controllers.API.V2.Internal.ItContracts
{
    [InternalApi]
    [Route("api/v2/internal/it-contracts/grid-roles")]
    public class GridLocalItContractRolesV2Controller : InternalApiV2Controller
    {
        private readonly IOptionsApplicationService<ItContractRight, ItContractRole> _optionService;

        public GridLocalItContractRolesV2Controller(IOptionsApplicationService<ItContractRight, ItContractRole> optionService)
        {
            _optionService = optionService;
        }

        [HttpGet]
        [Route("{organizationUuid}")]
        [ApiResponse(typeof(IEnumerable<LocalItContractRolesResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        public IActionResult GetByOrganizationUuid(Guid organizationUuid)
        {
            return _optionService.GetOptionTypes(organizationUuid)
                .Select(options => options.Select(x => x.Option))
                .Select(roles => roles
                    .Select(role => new LocalItContractRolesResponseDTO(role.Id, role.Uuid, role.Name))
                    .ToList())
                .Match(Ok, FromOperationError);
        }
    }
}
