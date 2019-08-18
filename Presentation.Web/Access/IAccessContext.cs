using Core.DomainModel;

namespace Presentation.Web.Access
{
    public interface IAccessContext
    {
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
        /// Determines if write-access is allowed for the provided entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool AllowUpdates(IEntity entity);
    }
}
