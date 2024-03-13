using Core.ApplicationServices.GDPR;
using Core.DomainServices.Queries.DPR;
using Core.DomainServices.Queries;
using Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystem;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Types.Shared;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.Abstractions.Extensions;
using Core.DomainModel.GDPR;
using Presentation.Web.Extensions;
using Presentation.Web.Models.API.V2.Response.DataProcessing;

namespace Presentation.Web.Controllers.API.V2.Internal.DataProcessingRegistrations
{
    /// <summary>
    /// Internal API for the data processing registrations stored in KITOS.
    /// </summary>
    [RoutePrefix("api/v2/internal/data-processing-registrations")]
    public class DataProcessingRegistrationInternalV2Controller : InternalApiV2Controller
    {
        private readonly IDataProcessingRegistrationApplicationService _dataProcessingRegistrationService;
        private readonly IDataProcessingRegistrationResponseMapper _responseMapper;

        public DataProcessingRegistrationInternalV2Controller(IDataProcessingRegistrationApplicationService dataProcessingRegistrationService, 
            IDataProcessingRegistrationResponseMapper responseMapper)
        {
            _dataProcessingRegistrationService = dataProcessingRegistrationService;
            _responseMapper = responseMapper;
        }

        /// <summary>
        /// Shallow search endpoint returning all Data Processing Registrations available to the current user
        /// </summary>
        /// <param name="nameContains">Include only dprs with a name that contains the content in the parameter</param>
        /// <param name="orderByProperty">Ordering property</param>
        /// <returns></returns>
        [HttpGet]
        [Route("search")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<DataProcessingRegistrationResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystems(
            [NonEmptyGuid] Guid organizationUuid,
            string nameContains = null,
            CommonOrderByProperty? orderByProperty = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var conditions = new List<IDomainQuery<DataProcessingRegistration>> { new QueryByOrganizationUuid<DataProcessingRegistration>(organizationUuid) };

            if(nameContains != null)
                conditions.Add(new QueryByPartOfName<DataProcessingRegistration>(nameContains));

            return _dataProcessingRegistrationService
                .Query(conditions.ToArray())
                .OrderApiResultsByDefaultConventions(false, orderByProperty)
                .Page(paginationQuery)
                .ToList()
                .Select(_responseMapper.MapDataProcessingRegistrationDTO)
                .ToList()
                .Transform(Ok);
        }
    }
}