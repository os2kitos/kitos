using Core.ApplicationServices.GDPR;
using Core.DomainModel.GDPR;
using Core.DomainServices.Queries;
using Core.DomainServices.Queries.DPR;
using Infrastructure.Services.Types;
using Presentation.Web.Controllers.API.V2.Mapping;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Queries;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations
{
    /// <summary>
    /// API for the data processing registrations in KITOS
    /// </summary>
    [RoutePrefix("api/v2/data-processing-registrations")]
    public class DataProcessingRegistrationV2Controller : ExternalBaseController
    {
        private readonly IDataProcessingRegistrationApplicationService _dataProcessingRegistrationService;

        public DataProcessingRegistrationV2Controller(IDataProcessingRegistrationApplicationService dataProcessingRegistrationService)
        {
            _dataProcessingRegistrationService = dataProcessingRegistrationService;
        }

        /// <summary>
        /// Returns all Data-Processing-Registrations in the requested organization available to the user
        /// </summary>
        /// <param name="organizationUuid">Organization UUID filter</param>
        /// <param name="systemUuid">System UUID filter</param>
        /// <param name="systemUsageUuid">SystemUsage UUID filter</param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetDataProcessingRegistrations(
            [NonEmptyGuid] Guid? organizationUuid = null,
            [NonEmptyGuid] Guid? systemUuid = null,
            [NonEmptyGuid] Guid? systemUsageUuid = null,
            [FromUri] BoundedPaginationQuery paginationQuery = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var conditions = new List<IDomainQuery<DataProcessingRegistration>>();

            if(organizationUuid.HasValue)
                conditions.Add(new QueryByOrganizationUuid<DataProcessingRegistration>(organizationUuid.Value));

            if (systemUuid.HasValue)
                conditions.Add(new QueryBySystemUuid(systemUuid.Value));

            if (systemUsageUuid.HasValue)
                conditions.Add(new QueryBySystemUsageUuid(systemUsageUuid.Value));

            return _dataProcessingRegistrationService
                .Query(conditions.ToArray())
                .OrderBy(dpr => dpr.Id)
                .Page(paginationQuery)
                .ToList()
                .Select(x => x.MapIdentityNamePairDTO())
                .Transform(Ok);
        }

        /// <summary>
        /// Returns a specific Data-Processing-Registration
        /// </summary>
        /// <param name="dataProcessingRegistraionUuid">UUID of Data-Processing-Registration entity</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{dataProcessingRegistraionUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityNamePairResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetItSystemUsage([NonEmptyGuid] Guid dataProcessingRegistraionUuid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _dataProcessingRegistrationService
                .GetByUuid(dataProcessingRegistraionUuid)
                .Select(x => x.MapIdentityNamePairDTO())
                .Match(Ok, FromOperationError);
        }

    }
}