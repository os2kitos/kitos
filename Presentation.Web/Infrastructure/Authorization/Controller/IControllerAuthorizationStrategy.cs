﻿using System;
using Core.DomainModel;
using Core.DomainServices.Authorization;

namespace Presentation.Web.Infrastructure.Authorization.Controller
{
    public interface IControllerAuthorizationStrategy
    {
        CrossOrganizationDataReadAccessLevel GetCrossOrganizationReadAccess();
        OrganizationDataReadAccessLevel GetOrganizationReadAccessLevel(int organizationId);
        [Obsolete("Use GetOrganizationReadAccessLevel")]
        bool AllowRead(IEntity entity);
        bool AllowCreate<T>(IEntity entity);
        bool AllowCreate<T>();
        bool AllowModify(IEntity entity);
        bool AllowDelete(IEntity entity);
        bool AllowEntityVisibilityControl(IEntity entity);
    }
}
