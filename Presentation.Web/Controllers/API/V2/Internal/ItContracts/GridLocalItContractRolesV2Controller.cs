using Core.DomainModel.ItContract;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Generic;
using Presentation.Web.Infrastructure.Attributes;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using System.Net;
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ItContractRole>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetByOrganizationUuid(Guid organizationUuid)
        {
            return _gridLocalItContractRolesService.GetOverviewRoles(organizationUuid).Match(Ok, FromOperationError);
        }
    }
}