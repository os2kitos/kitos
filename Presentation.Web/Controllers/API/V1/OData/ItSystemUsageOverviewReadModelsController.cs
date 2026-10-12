using System.Linq;
using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.SystemUsage.ReadModels;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [InternalApi]
    public class ItSystemUsageOverviewReadModelsController : BaseOdataController
    {
        private readonly IItsystemUsageOverviewReadModelsService _readModelsService;
        private readonly IEntityIdentityResolver _identityResolver;

        public ItSystemUsageOverviewReadModelsController(IItsystemUsageOverviewReadModelsService readModelsService, IEntityIdentityResolver identityResolver)
        {
            _readModelsService = readModelsService;
            _identityResolver = identityResolver;
        }

        [EnableQuery]
        [Route("odata/Organizations({organizationId})/ItSystemUsageOverviewReadModels")]
        public IActionResult GetByOrganizationId([FromRoute] int organizationId, int? responsibleOrganizationUnitId = null)
        {
            return GetOverviewReadModels(organizationId, responsibleOrganizationUnitId);
        }

        /// <summary>
        /// V2 style OData endpoint suited for consumption by clients using UUID's for entity identity
        /// </summary>
        /// <param name="organizationUuid"></param>
        /// <param name="responsibleOrganizationUnitUuid"></param>
        /// <returns></returns>
        [EnableQuery(MaxAnyAllExpressionDepth = 3, MaxNodeCount = 300)]
        [Route("odata/ItSystemUsageOverviewReadModels")]
        public IActionResult Get(Guid organizationUuid, Guid? responsibleOrganizationUnitUuid = null)
        {
            var orgDbId = _identityResolver.ResolveDbId<Organization>(organizationUuid);
            if (orgDbId.IsNone)
            {
                return FromOperationError(new OperationError("Invalid org id", OperationFailure.NotFound));
            }

            if (!responsibleOrganizationUnitUuid.HasValue) return GetOverviewReadModels(orgDbId.Value, null);

            var unitDbId = _identityResolver.ResolveDbId<OrganizationUnit>(responsibleOrganizationUnitUuid.Value);
            return unitDbId.IsNone 
                ? FromOperationError(new OperationError("Invalid org unit id", OperationFailure.BadInput)) 
                : GetOverviewReadModels(orgDbId.Value, unitDbId.Value);
        }

        private IActionResult GetOverviewReadModels(int organizationId, int? responsibleOrganizationUnitId)
        {
            var byOrganizationId = responsibleOrganizationUnitId == null
                ? _readModelsService.GetByOrganizationId(organizationId)
                : _readModelsService.GetByOrganizationAndResponsibleOrganizationUnitId(organizationId,
                    responsibleOrganizationUnitId.Value);
            return
                byOrganizationId
                    .Match(onSuccess: q => Ok(q.ToList()), onFailure: FromOperationError);
        }
    }
}



