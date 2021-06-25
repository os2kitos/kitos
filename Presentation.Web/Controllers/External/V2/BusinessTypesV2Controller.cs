using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel.ItSystem;
using Presentation.Web.Extensions;
using Presentation.Web.Models.External.V2;
using Presentation.Web.Models.External.V2.Request;
using Presentation.Web.Models.External.V2.Response;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.External.V2
{
    [RoutePrefix("api/v2/business-types")]
    public class BusinessTypesV2Controller: ExternalBaseController
    {
        private readonly IOptionsApplicationService<ItSystem, BusinessType> _businessTypeApplicationService;

        public BusinessTypesV2Controller(IOptionsApplicationService<ItSystem, BusinessType> businessTypeApplicationService)
        {
            _businessTypeApplicationService = businessTypeApplicationService;
        }


        /// <summary>
        /// Returns IT-System business types
        /// </summary>
        /// <returns>A list of available IT-System business type specifics formatted as uuid and name pairs</returns>
        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentityNamePairResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetBusinessTypes(Guid organizationUuid, [FromUri] StandardPaginationQuery pagination = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _businessTypeApplicationService
                .GetOptionTypes(organizationUuid)
                .Select(x => x.Page(pagination))
                .Select(ToDTOs)
                .Match(Ok, FromOperationError);
        }

        /// <summary>
        /// Returns requested IT-System business type
        /// </summary>
        /// <param name="businessTypeUuid">business type identifier</param>
        /// <returns>A uuid and name pair with boolean to mark if the business type is available in the organization</returns>
        [HttpGet]
        [Route("{businessTypeUuid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(AvailableNamePairResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public IHttpActionResult GetBusinessType(Guid businessTypeUuid, Guid organizationUuid)
        {
            return _businessTypeApplicationService
                .GetOptionType(organizationUuid, businessTypeUuid)
                .Select(x => ToAvailableDTO(x.option, x.available))
                .Match(Ok, FromOperationError);
        }

        private List<IdentityNamePairResponseDTO> ToDTOs(IEnumerable<BusinessType> businessTypes)
        {
            return businessTypes.Select(ToDTO).ToList();
        }

        private static IdentityNamePairResponseDTO ToDTO(BusinessType businessType)
        {
            return new(businessType.Uuid, businessType.Name);
        }

        private static AvailableNamePairResponseDTO ToAvailableDTO(BusinessType businessType, bool isAvailable)
        {
            return new(businessType.Uuid, businessType.Name, isAvailable);
        }
    }
}