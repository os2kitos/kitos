﻿using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage;
using System.Collections.Generic;

namespace Presentation.Web.Controllers.API.V2.Internal.ItSystemUsages.Mapping
{
    public interface IItSystemUsageMigrationResponseMapper
    {
        ItSystemUsageMigrationV2ResponseDTO MapMigration(ItSystemUsageMigration entity);

        ItSystemUsageMigrationPermissionsResponseDTO MapCommandPermissions(IEnumerable<CommandPermissionResult> permissionResult);
    }
}