using System;
using System.Web.Http;
using Core.Abstractions.Types;
using Core.ApplicationServices.SystemUsage.ReadModels;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;

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
        [ODataRoute("Organizations({organizationId})/ItSystemUsageOverviewReadModels")]
        public IHttpActionResult Get([FromODataUri] int organizationId, int? responsibleOrganizationUnitId = null)
        {
            return GetOverviewReadModels(organizationId, responsibleOrganizationUnitId);
        }

        /// <summary>
        /// V2 style OData endpoint suited for consumption by clients using UUID's for entity identity
        /// </summary>
        /// <param name="organizationUuid"></param>
        /// <param name="responsibleOrganizationUnitUuid"></param>
        /// <returns></returns>
        [EnableQuery]
        [ODataRoute("ItSystemUsageOverviewReadModels")]
        public IHttpActionResult GetByUuid(Guid organizationUuid, Guid? responsibleOrganizationUnitUuid = null)
        {
            var orgDbId = _identityResolver.ResolveDbId<Organization>(organizationUuid);
            if (orgDbId.IsNone)
            {
                return FromOperationError(new OperationError("Invalid org id", OperationFailure.NotFound));
            }

            int? orgUnitId = null;
            if (responsibleOrganizationUnitUuid.HasValue)
            {
                var unitDbId = _identityResolver.ResolveDbId<OrganizationUnit>(responsibleOrganizationUnitUuid.Value);
                if (unitDbId.IsNone)
                {
                    return FromOperationError(new OperationError("Invalid org unit id", OperationFailure.BadInput));
                }

                orgUnitId = unitDbId.Value;
            }

            return GetOverviewReadModels(orgDbId.Value, orgUnitId);
        }

        private IHttpActionResult GetOverviewReadModels(int organizationId, int? responsibleOrganizationUnitId)
        {
            var byOrganizationId = responsibleOrganizationUnitId == null
                ? _readModelsService.GetByOrganizationId(organizationId)
                : _readModelsService.GetByOrganizationAndResponsibleOrganizationUnitId(organizationId,
                    responsibleOrganizationUnitId.Value);
            return
                byOrganizationId
                    .Match(onSuccess: Ok, onFailure: FromOperationError);
        }
    }
}