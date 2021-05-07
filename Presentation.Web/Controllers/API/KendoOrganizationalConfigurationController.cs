using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Core.ApplicationServices;
using Core.DomainModel;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    [RoutePrefix("api/v1/kendo-organizational-configuration")]
    public class KendoOrganizationalConfigurationController : BaseApiController
    {
        private readonly IKendoOrganizationalConfigurationService _kendoOrganizationalConfigurationService;

        public KendoOrganizationalConfigurationController(IKendoOrganizationalConfigurationService kendoOrganizationalConfigurationService)
        {
            _kendoOrganizationalConfigurationService = kendoOrganizationalConfigurationService;
        }

        protected override bool AllowCreateNewEntity(int organizationId) => AllowCreate<KendoOrganizationalConfiguration>(organizationId);

        [HttpPost]
        [Route]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ApiReturnDTO<KendoOrganizationalConfigurationDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public HttpResponseMessage SaveConfiguration([FromBody] KendoOrganizationalConfigurationDTO dto)
        {
            if (dto == null)
                return BadRequest("No input parameters provided");

            return _kendoOrganizationalConfigurationService
                .CreateOrUpdate(dto.OrganizationId, dto.OverviewType, dto.Configuration)
                .Match(value => Ok(ToDTO(value)), FromOperationError);
        }

        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ApiReturnDTO<KendoOrganizationalConfigurationDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetConfiguration([FromUri] int organizationId, [FromUri] OverviewType overviewType)
        {
            if (organizationId == null || overviewType == null)
                return BadRequest("Please provide both organizationId and overviewType");

            return _kendoOrganizationalConfigurationService
                .Get(organizationId, overviewType)
                .Match(value => Ok(ToDTO(value)), FromOperationError);
        }

        private KendoOrganizationalConfigurationDTO ToDTO(KendoOrganizationalConfiguration value)
        {
            return new KendoOrganizationalConfigurationDTO
            {
                OrganizationId = value.OrganizationId,
                OverviewType = value.OverviewType,
                Configuration = value.Configuration
            };
        }
    }
}