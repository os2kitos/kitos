using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Shared;
using Presentation.Web.Models.API.V2.Response.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public interface IResourcePermissionsResponseMapper
    {
        ResourcePermissionsResponseDTO Map(ResourcePermissionsResult permissionsResult);
        CombinedPermissionsResponseDTO Map(CombinedPermissionsResult permissionsResult);
        ResourceCollectionPermissionsResponseDTO Map(ResourceCollectionPermissionsResult permissionsResult);
    }
}
