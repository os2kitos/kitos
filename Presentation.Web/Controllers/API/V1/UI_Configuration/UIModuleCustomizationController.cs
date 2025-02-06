using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.Model.UiCustomization;
using Core.ApplicationServices.UIConfiguration;
using Core.DomainModel.UIConfiguration;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.UI_Configuration;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1.UI_Configuration
{
    [InternalApi]
    [RoutePrefix("api/v1/organizations/{organizationId}/ui-config/modules/{module}")]
    public class UIModuleCustomizationController : BaseApiController
    {
        private readonly IUIModuleCustomizationService _uiModuleCustomizationServiceService;

        public UIModuleCustomizationController(IUIModuleCustomizationService uiModuleCustomizationServiceService)
        {
            _uiModuleCustomizationServiceService = uiModuleCustomizationServiceService;
        }

        [HttpGet]
        [Route]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<UIModuleCustomizationDTO>))]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public HttpResponseMessage Get(int organizationId, string module)
        {
            return _uiModuleCustomizationServiceService
                .GetModuleCustomizationForOrganization(organizationId, module)
                .Select(ToDto)
                .Match(dto => dto.Nodes.Any() ? Ok(dto) : NotFound(), FromOperationError);
        }

        [HttpPut]
        [Route]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Forbidden)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public HttpResponseMessage Put(int organizationId, string module, [FromBody] UIModuleCustomizationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return _uiModuleCustomizationServiceService
                .UpdateModule(PrepareParameters(dto, organizationId, module)
                )
                .Match(FromOperationError, NoContent);
        }
        
        private static UIModuleCustomizationParameters PrepareParameters(
            UIModuleCustomizationDTO value,
            int organizationId,
            string module)
        {
            return new UIModuleCustomizationParameters(organizationId, module,
                value.Nodes.Select(PrepareNodeParameters).ToList());
        }

        private static CustomUINodeParameters PrepareNodeParameters(
            CustomizedUINodeDTO value)
        {
            return new CustomUINodeParameters(value.Key, value.Enabled);
        }

        private static UIModuleCustomizationDTO ToDto(
            UIModuleCustomization value)
        {
            return new UIModuleCustomizationDTO()
            {
                Nodes = value.Nodes.Select(NodeToDto).ToList()
            };
        }

        private static CustomizedUINodeDTO NodeToDto(
            CustomizedUINode value)
        {
            return new CustomizedUINodeDTO()
            {
                Key = value.Key,
                Enabled = value.Enabled
            };
        }
    }
}