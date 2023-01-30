using Core.Abstractions.Types;
using Core.DomainModel;

namespace Core.ApplicationServices.Authorization
{
    public class ResourcePermissionsResult
    {
        private static readonly ResourcePermissionsResult Empty = new(false, false, false);

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
                .Select(entity => new ResourcePermissionsResult(true, authorizationContext.AllowModify(entity), authorizationContext.AllowDelete(entity)))
                .Match<Result<ResourcePermissionsResult, OperationError>>
                (
                    result => result,
                    error => error.FailureType == OperationFailure.Forbidden ? Empty : error
                );
        }
    }
}
