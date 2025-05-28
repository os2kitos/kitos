using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using PubSub.Application.Api.Attributes;

public class ApiVersioningConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            var versionAttr = controller.Attributes
                .OfType<ApiVersionAttribute>()
                .FirstOrDefault();
            if (versionAttr == null)
                continue;

            var prefixRoute = new AttributeRouteModel(
                new RouteAttribute($"api/v{versionAttr.Version}")
            );

            foreach (var selector in controller.Selectors
                         .Where(s => s.AttributeRouteModel != null))
            {
                selector.AttributeRouteModel =
                    AttributeRouteModel.CombineAttributeRouteModel(
                        prefixRoute,
                        selector.AttributeRouteModel
                    );
            }
        }
    }
}