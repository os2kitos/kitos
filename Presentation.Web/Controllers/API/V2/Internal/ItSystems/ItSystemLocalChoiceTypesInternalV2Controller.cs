

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Core.ApplicationServices.LocalOptions;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Response.Options;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V2.Internal.ItSystems
{
    [RoutePrefix("api/v2/internal/it-systems/{organizationUuid}/local-choice-types")]
    public class ItSystemLocalChoiceTypesInternalV2Controller: InternalApiV2Controller
    {

        private readonly IGenericLocalOptionsService<LocalBusinessType, ItSystem, BusinessType> _businessTypeService;

        public ItSystemLocalChoiceTypesInternalV2Controller(IGenericLocalOptionsService<LocalBusinessType, ItSystem, BusinessType> businessTypeService)
        {
            _businessTypeService = businessTypeService;
        }

        [HttpGet]
        [Route("business-types")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<RegularOptionResponseDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        public IHttpActionResult GetBusinessTypes([NonEmptyGuid][FromUri] Guid organizationUuid)
        {
            if (!ModelState.IsValid) return BadRequest();

            return _businessTypeService.GetByOrganizationUuid(organizationUuid)
                .Select(ToRegularOptionDTOS)
                .Match(Ok, FromOperationError);
        }

        private IEnumerable<RegularOptionResponseDTO> ToRegularOptionDTOS<TOption>(IEnumerable<TOption> options)
            where TOption : OptionEntity<ItSystem>
        {
            return options.Select(ToRegularOptionDTO);
        }

        private RegularOptionResponseDTO ToRegularOptionDTO<TOption>(TOption option)
        where TOption: OptionEntity<ItSystem>
        {
            return new(option.Uuid, option.Name, option.Description);
        }
    }
}