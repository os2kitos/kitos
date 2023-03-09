using Core.ApplicationServices.Authorization;
using Presentation.Web.Models.API.V2.Response.Shared;

namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public class ResourcePermissionsResponseMapper : IResourcePermissionsResponseMapper
    {
        public ResourcePermissionsResponseDTO Map(ResourcePermissionsResult permissionsResult)
        {
            return new ResourcePermissionsResponseDTO
            {
                Delete = permissionsResult.Delete,
                Modify = permissionsResult.Modify,
                Read = permissionsResult.Read
            };
        }
        public ResourceCollectionPermissionsResponseDTO Map(ResourceCollectionPermissionsResult permissionsResult)
        {
            return new ResourceCollectionPermissionsResponseDTO
            {
                Create = permissionsResult.Create
            };
        }
    }
}