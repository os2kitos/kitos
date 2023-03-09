﻿using System.Collections.Generic;
using Core.ApplicationServices.Authorization;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public interface IResourcePermissionsResponseMapper
    {
        ResourcePermissionsResponseDTO Map(ResourcePermissionsResult permissionsResult);
        ResourceCollectionPermissionsResponseDTO Map(ResourceCollectionPermissionsResult permissionsResult);
        TResult MapCommandPermissions<TResult>(IEnumerable<CommandPermissionResult> permissionResult) where TResult : class, IHasCommandPermissionsResponseDTO, new();
    }
}
