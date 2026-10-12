using Core.Abstractions.Types;

namespace Core.DomainModel.LocalOptions
{
    public class LocalRoleOptionEntity<TOptionType> : LocalOptionEntity<TOptionType>
    {
        public bool IsExternallyUsed { get; set; }
        public string ExternallyUsedDescription { get; set; }

        public void UpdateIsExternallyUsed(Maybe<bool> isExternallyUsed)
        {
            if (isExternallyUsed.HasValue)
                IsExternallyUsed = isExternallyUsed.Value;
            if (!IsExternallyUsed)
                ExternallyUsedDescription = null;
        }

        public Maybe<OperationError> UpdateExternallyUsedDescription(Maybe<string> externallyUsedDescription)
        {
            if (externallyUsedDescription.IsNone)
                return Maybe<OperationError>.None;
            if (!IsExternallyUsed)
                return new OperationError("Cannot add externally used description if option is not used externally", OperationFailure.BadState);
            ExternallyUsedDescription = externallyUsedDescription.Value;
            return Maybe<OperationError>.None;
        }
    }
}
