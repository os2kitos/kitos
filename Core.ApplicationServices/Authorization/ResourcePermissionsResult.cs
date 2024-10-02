using Core.Abstractions.Types;
using Core.DomainModel;

namespace Core.ApplicationServices.Authorization
{
    public class ResourcePermissionsResult
    {
        protected static readonly ResourcePermissionsResult Empty = new(false, false, false);

        public bool Read { get; }
        public bool Modify { get; }
        public bool Delete { get; }

        public ResourcePermissionsResult(bool read, bool modify, bool delete)
        {
            Read = read;
            Modify = modify;
            Delete = delete;
        }

        public static Result<ResourcePermissionsResult, OperationError> FromResolutionResult<T>(
            Result<T, OperationError> getEntityResult,
            IAuthorizationContext authorizationContext) where T : IEntity
        {
            return getEntityResult
                .Select(entity => FromEntity(entity, authorizationContext))
                .Match
                (
                    result => result,
                    error => error.FailureType == OperationFailure.Forbidden ? Empty : error
                );
        }

        public static Result<ResourcePermissionsResult, OperationError> FromEntity<T>(
            T entity,
            IAuthorizationContext authorizationContext) where T : IEntity
        {
            return new ResourcePermissionsResult(authorizationContext.AllowReads(entity), authorizationContext.AllowModify(entity), authorizationContext.AllowDelete(entity));
        }
    }
}
