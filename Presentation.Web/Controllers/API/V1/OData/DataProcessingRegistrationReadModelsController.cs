using System;
using System.Net;
using System.Web.Http;
using Core.Abstractions.Types;
using Core.ApplicationServices.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Presentation.Web.Infrastructure.Attributes;
using Swashbuckle.OData;
using Swashbuckle.Swagger.Annotations;

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
        [SwaggerResponse(HttpStatusCode.OK, type:typeof(ODataListResponse<DataProcessingRegistrationReadModel>))]
        [ODataRoute("Organizations({organizationId})/DataProcessingRegistrationReadModels")]
        public IHttpActionResult Get([FromODataUri]int organizationId)
        {
            return GetOverviewReadModels(organizationId);
        }

        /// <summary>
        /// V2 style OData endpoint suited for consumption by clients using UUID's for entity identity
        /// </summary>
        /// <param name="organizationUuid"></param>
        /// <returns></returns>
        [EnableQuery(MaxNodeCount = 300)]
        [SwaggerResponse(HttpStatusCode.OK, type: typeof(ODataListResponse<DataProcessingRegistrationReadModel>))]
        [ODataRoute("DataProcessingRegistrationReadModels")]
        public IHttpActionResult GetByUuid(Guid organizationUuid, Guid? responsibleOrganizationUnitUuid = null)
        {
            var orgDbId = _identityResolver.ResolveDbId<Organization>(organizationUuid);
            if (orgDbId.IsNone)
            {
                return FromOperationError(new OperationError("Invalid org id", OperationFailure.NotFound));
            }

            return GetOverviewReadModels(orgDbId.Value);
        }

        private IHttpActionResult GetOverviewReadModels(int organizationId)
        {
            return _dataProcessingRegistrationReadModelService
                .GetByOrganizationId(organizationId)
                .Match(onSuccess: Ok, onFailure: FromOperationError);
        }
    }
}
