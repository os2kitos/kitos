using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.ApplicationServices.UIConfiguration;
using Core.DomainModel.UIConfiguration;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.UI_Configuration;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API.V1.UI_Configuration;

[InternalApi]
[RoutePrefix("api/v1/organizations/{organizationId}/ui-config/modules/{module}")]
public class UIModuleConfigurationController : BaseApiController
{
    private readonly IUIModuleCustomizationService _uiVisibilityConfigurationService;

    public UIModuleConfigurationController(IUIModuleCustomizationService uiVisibilityConfigurationService)
    {
        _uiVisibilityConfigurationService = uiVisibilityConfigurationService;
    }

    [HttpGet]
    [Route]
    [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<UIModuleCustomizationDTO>))]
    [SwaggerResponse(HttpStatusCode.Forbidden)]
    [SwaggerResponse(HttpStatusCode.NotFound)]
    [SwaggerResponse(HttpStatusCode.Unauthorized)]
    public HttpResponseMessage Get(int organizationId, string module)
    {
        return _uiVisibilityConfigurationService
            .GetModuleConfigurationForOrganization(organizationId, module)
            .Match(entities => Ok(entities.Select(ToDto).ToList()), FromOperationError);
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
        return _uiVisibilityConfigurationService
            .Put(
                organizationId, 
                module, 
                ToEntity(dto, organizationId, module)
                )
            .Match(_ => NoContent(), FromOperationError);
    }

    private static UIModuleCustomization ToEntity(
        UIModuleCustomizationDTO value,
        int organizationId,
        string module)
    {
        return new UIModuleCustomization
        {
            OrganizationId = organizationId,
            Module = module,
            Nodes = value.Nodes.Select(NodeToEntity).ToList()
        };
    }

    private static CustomizedUINode NodeToEntity(
        CustomizedUINodeDTO value)
    {
        return new CustomizedUINode()
        {
            Key = value.Key,
            Enabled = value.Enabled
        };
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