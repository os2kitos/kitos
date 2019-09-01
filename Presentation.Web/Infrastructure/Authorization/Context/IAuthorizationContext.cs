using Core.DomainModel;
using Core.DomainServices.Authorization;

namespace Presentation.Web.Infrastructure.Authorization.Context
{
    public interface IAuthorizationContext
    {
        /// <summary>
        /// Determine the granularity of cross organization read access supported by the current authorization context
        /// </summary>
        /// <returns></returns>
        CrossOrganizationReadAccess GetCrossOrganizationReadAccess();
        /// <summary>
        /// Determines if high level read-access is allowed for objects within the target organizational context
        /// NOTE: Does not provide entity-level access rights. Just answers the question if ANY access at all can be granted.
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        bool AllowReadsWithinOrganization(int organizationId);
        /// <summary>
        /// Determines if read-access is allowed for the provided entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool AllowReads(IEntity entity);
        /// <summary>
        /// Determines if create-access is allowed for the provided entity type
        /// </summary>
        /// <param name="entity"></param>
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
    }
}
