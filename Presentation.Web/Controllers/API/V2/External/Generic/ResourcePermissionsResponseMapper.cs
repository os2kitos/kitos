using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Shared;
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

        public CombinedPermissionsResponseDTO Map(CombinedPermissionsResult permissionsResult)
        {
            return new CombinedPermissionsResponseDTO
            {
                Delete = permissionsResult.BasePermissions.Delete,
                Modify = permissionsResult.BasePermissions.Modify,
                Read = permissionsResult.BasePermissions.Read,
                FieldPermissions = Map(permissionsResult.FieldPermissions)
            };
        }

        private ModuleFieldPermissionsResponseDTO Map(ModuleFieldsPermissionsResult fieldPermissionsResult)
        {
            return new ModuleFieldPermissionsResponseDTO
            {
                Fields = fieldPermissionsResult.Fields.Select(Map).ToList()
            };
        }

        private FieldPermissionsResponseDTO Map(FieldPermissionsResult fieldPermissionsResult)
        {
            return new FieldPermissionsResponseDTO
            {
                Key = fieldPermissionsResult.Key,
                Enabled = fieldPermissionsResult.Enabled
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