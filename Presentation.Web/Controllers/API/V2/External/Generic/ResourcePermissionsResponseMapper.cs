using Core.ApplicationServices.Authorization;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.SharedProperties;
using System.Collections.Generic;
using System.Linq;

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

        public TResult MapCommandPermissions<TResult>(IEnumerable<CommandPermissionResult> permissionResult) where TResult : class, IHasCommandPermissionsResponseDTO, new()
        {
            return new TResult
            {
                Commands = permissionResult.Select(x =>
                    new CommandPermissionResponseDTO
                    {
                        Id = x.Id,
                        CanExecute = x.CanExecute
                    })
            };
        }
    }
}