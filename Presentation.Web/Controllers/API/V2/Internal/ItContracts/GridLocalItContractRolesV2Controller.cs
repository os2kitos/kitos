using System;
using Presentation.Web.Infrastructure.Attributes;
using System.Web.Http;
using Core.ApplicationServices.Contract.ReadModels;

namespace Presentation.Web.Controllers.API.V2.Internal.ItContracts
{
    [InternalApi]
    [RoutePrefix("api/v2/internal/it-contracts/grid-roles")]
    public class GridLocalItContractRolesV2Controller : InternalApiV2Controller
    {
        private readonly IGridLocalItContractRolesService _gridLocalItContractRolesService;
        public GridLocalItContractRolesV2Controller(IGridLocalItContractRolesService gridLocalItContractRolesService)
        {
            _gridLocalItContractRolesService = gridLocalItContractRolesService;
        }


        [HttpGet]
        [Route("{organizationUuid}")]
        public IHttpActionResult GetByOrganizationUuid(Guid organizationUuid)
        {
            return _gridLocalItContractRolesService.GetOverviewRoles(organizationUuid).Match(Ok, FromOperationError);
        }
    }
}