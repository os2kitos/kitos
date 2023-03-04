using Core.ApplicationServices.Authorization;
using Presentation.Web.Models.API.V2.Response.Shared;

namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public interface IResourcePermissionsResponseMapper
    {
        ResourcePermissionsResponseDTO Map(ResourcePermissionsResult permissionsResult);
        ResourceCollectionPermissionsResponseDTO Map(ResourceCollectionPermissionsResult permissionsResult);
    }
}
