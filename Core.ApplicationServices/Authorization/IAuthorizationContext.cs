﻿using Core.DomainModel;
using Core.DomainServices.Authorization;

namespace Core.ApplicationServices.Authorization
{
    public interface IAuthorizationContext
    {
        /// <summary>
        /// Determine the granularity of cross organization read access supported by the current authorization context
        /// </summary>
        /// <returns></returns>
        CrossOrganizationDataReadAccessLevel GetCrossOrganizationReadAccess();
        /// <summary>
        /// Determines, at a high level, the depth of read-access which is allowed on objects within the target organization wrt. the active organization.
        /// NOTE: Does not provide entity-level access rights. Just answers the question if ANY access at all can be granted.
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        OrganizationDataReadAccessLevel GetOrganizationReadAccessLevel(int organizationId);
        /// <summary>
        /// Determines if read-access is allowed for the provided entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool AllowReads(IEntity entity);
        /// <summary>
        /// Determines if create-access is allowed for the provided entity type
        /// </summary>
        /// <returns></returns>
        bool AllowCreate<T>();
        /// <summary>
        /// Determines if create-access is allowed for the provided entity type and with the representation passed in <paramref name="entity"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool AllowCreate<T>(IEntity entity);
        /// <summary>
        /// Determines if update-access is allowed for the provided entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool AllowModify(IEntity entity);
        /// <summary>
        /// Determines if delete-access is allowed for the provided entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool AllowDelete(IEntity entity);
        /// <summary>
        /// Determines if write-access is allowed to entity's visibility control
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool AllowEntityVisibilityControl(IEntity entity);
        /// <summary>
        /// Determines if the current context allows system migration
        /// </summary>
        /// <returns></returns>
        bool AllowSystemUsageMigration();
    }
}
