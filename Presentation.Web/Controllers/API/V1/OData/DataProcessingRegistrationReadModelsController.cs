using System.Linq;
using System;
using System.Net;
using Core.Abstractions.Types;
using Core.ApplicationServices.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Presentation.Web.Controllers.API.V1.OData
{
    /// <summary>
    /// Search API used for DataProcessingRegistrations
    /// </summary>
    [InternalApi]
    public class DataProcessingRegistrationReadModelsController : BaseOdataController
    {
        private readonly IDataProcessingRegistrationReadModelService _dataProcessingRegistrationReadModelService;
        private readonly IEntityIdentityResolver _identityResolver;

        public DataProcessingRegistrationReadModelsController(IDataProcessingRegistrationReadModelService dataProcessingRegistrationReadModelService, 
             IEntityIdentityResolver identityResolver)
        {
            _dataProcessingRegistrationReadModelService = dataProcessingRegistrationReadModelService;
            _identityResolver = identityResolver;
        }

        [EnableQuery]
        [Route("odata/Organizations({organizationId})/DataProcessingRegistrationReadModels")]
        public IActionResult GetByOrganizationId([FromRoute]int organizationId)
        {
            return GetOverviewReadModels(organizationId);
        }

        /// <summary>
        /// V2 style OData endpoint suited for consumption by clients using UUID's for entity identity
        /// </summary>
        /// <param name="organizationUuid">UUID of the organization</param>
        /// <param name="responsibleOrganizationUnitUuid">UUID of the responsible organization unit</param>
        /// <returns></returns>
        [EnableQuery(MaxNodeCount = 300)]
        [Route("odata/DataProcessingRegistrationReadModels")]
        public IActionResult Get(Guid organizationUuid, Guid? responsibleOrganizationUnitUuid = null)
        {
            var orgDbId = _identityResolver.ResolveDbId<Organization>(organizationUuid);
            if (orgDbId.IsNone)
            {
                return FromOperationError(new OperationError("Invalid org id", OperationFailure.NotFound));
            }

            return GetOverviewReadModels(orgDbId.Value);
        }

        private IActionResult GetOverviewReadModels(int organizationId)
        {
            return _dataProcessingRegistrationReadModelService
                .GetByOrganizationId(organizationId)
                .Match(onSuccess: q => Ok(q.ToList()), onFailure: FromOperationError);
        }
    }
}




