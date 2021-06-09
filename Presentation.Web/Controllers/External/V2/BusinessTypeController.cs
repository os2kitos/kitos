using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.LocalOption;
using Core.DomainModel.ItSystem;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [PublicApi]
    [RoutePrefix("api/v2/business-types")]
    public class BusinessTypeController: ExternalBaseController
    {
        private readonly IBusinessTypeApplicationService _businessTypeApplicationService;

        public BusinessTypeController(IBusinessTypeApplicationService businessTypeApplicationService)
        {
            _businessTypeApplicationService = businessTypeApplicationService;
        }


        /// <summary>
        /// Returns IT-System business types
        /// </summary>
        /// <param name="organizationUuid">organization identifier</param>
        /// <returns>A list of available IT-System business type specifics formatted as uuid and name pairs</returns>
        [HttpGet]
        [Route("organisation/{organizationUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public HttpResponseMessage GetBusinessTypes(Guid organizationUuid)
        {
            return _businessTypeApplicationService
                .GetBusinessTypes(organizationUuid)
                .Match(value => Ok(ToDTOs(value)), FromOperationError);
        }

        /// <summary>
        /// Returns requested IT-System business type
        /// </summary>
        /// <param name="organizationUuid">organization identifier</param>
        /// <param name="businessTypeUuid">business type identifier</param>
        /// <returns>A uuid and name pair with boolean to mark if the business type is available in the organization</returns>
        [HttpGet]
        [Route("organisation/{organizationUuid}/{businessTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(AvailableNamePairResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetBusinessType(Guid organizationUuid, Guid businessTypeUuid)
        {
            return _businessTypeApplicationService
                .GetBusinessType(organizationUuid, businessTypeUuid)
                .Match(value => Ok(ToAvailableDTO(value.option, value.available)), FromOperationError);
        }

        private List<IdentityNamePairResponseDTO> ToDTOs(IEnumerable<BusinessType> businessTypes)
        {
            return businessTypes.Select(x => ToDTO(x)).ToList();
        }

        private IdentityNamePairResponseDTO ToDTO(BusinessType businessType)
        {
            return new IdentityNamePairResponseDTO(businessType.Uuid, businessType.Name);
        }

        private AvailableNamePairResponseDTO ToAvailableDTO(BusinessType businessType, bool isAvailable)
        {
            return new AvailableNamePairResponseDTO(businessType.Uuid, businessType.Name, isAvailable);
        }
    }
}