using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices;
using Core.DomainModel;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;
using Presentation.Web.Models.API.V1;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1
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

            var columns = dto.VisibleColumns.Select(x => new KendoColumnConfiguration(){
                PersistId = x.PersistId,
                Index = x.Index
                });

            return _kendoOrganizationalConfigurationService
                .CreateOrUpdate(dto.OrganizationId, dto.OverviewType, columns)
                .Match(value => Ok(Map(value)), FromOperationError);
        }

        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ApiReturnDTO<KendoOrganizationalConfigurationDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetConfiguration([FromUri][Required] int? organizationId, [FromUri][Required] OverviewType? overviewType)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _kendoOrganizationalConfigurationService
                .Get(organizationId.Value, overviewType.Value)
                .Match(value => Ok(Map(value)), FromOperationError);
        }

        [HttpGet]
        [Route("version")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ApiReturnDTO<string>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetConfigurationVersion([FromUri][Required] int? organizationId, [FromUri][Required] OverviewType? overviewType)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _kendoOrganizationalConfigurationService
                .Get(organizationId.Value, overviewType.Value)
                .Select(x => x.Version)
                .Match(Ok, FromOperationError);
        }

        [HttpDelete]
        [Route]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ApiReturnDTO<KendoOrganizationalConfigurationDTO>))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage DeleteConfiguration([FromUri][Required] int? organizationId, [FromUri][Required] OverviewType? overviewType)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _kendoOrganizationalConfigurationService
                .Delete(organizationId.Value, overviewType.Value)
                .Match(_ => Ok(), FromOperationError);
        }

        private static KendoOrganizationalConfigurationDTO Map(KendoOrganizationalConfiguration value)
        {
            return new KendoOrganizationalConfigurationDTO
            {
                OrganizationId = value.OrganizationId,
                OverviewType = value.OverviewType,
                VisibleColumns = value.VisibleColumns.Select(x => Map(x)).ToList(),
                Version = value.Version
            };
        }

        private static KendoColumnConfigurationDTO Map(KendoColumnConfiguration value)
        {
            return new KendoColumnConfigurationDTO
            {
                PersistId = value.PersistId,
                Index = value.Index
            };
        }
    }
}