using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;

namespace Core.ApplicationServices.Authorization
{
    public interface IAuthorizationContext
    {
        /// <summary>
        /// Gets the authorization model fitting for the current context
        /// </summary>
        IAuthorizationModel GetAuthorizationModel(IEntityOwnedByOrganization entity);
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
        /// Determines read access level of the entity type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        EntityReadAccessLevel GetReadAccessLevel<T>();
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
        bool AllowCreate<T>(int organizationId);
        /// <summary>
        /// Determines if create-access is allowed for the provided entity type and with the representation passed in <paramref name="entity"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="organizationId"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool AllowCreate<T>(int organizationId, IEntity entity);
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
        /// Determines if the current context allows for the permission
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        bool HasPermission(Permission permission);

        /// <summary>
        /// Determines if the user in the current context is allowed to administer the provided organization right
        /// </summary>
        /// <param name="right"></param>
        /// <returns></returns>
        bool AllowAdministerOrganizationRight(OrganizationRight right);
    }
}
