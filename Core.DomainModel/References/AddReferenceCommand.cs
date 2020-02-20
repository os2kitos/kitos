using System;
using Core.DomainModel.Result;

namespace Core.DomainModel.References
{
    /// <summary>
    /// Performs the "Add reference command" which involves adding a reference
    /// as well as marking the new reference as "master" if it is the only one
    /// </summary>
    public class AddReferenceCommand
    {
        private readonly IEntityWithExternalReferences _target;

        public AddReferenceCommand(IEntityWithExternalReferences target)
        {
            _target = target;
        }

        public Result<ExternalReference, OperationError> AddExternalReference(ExternalReference newReference)
        {
            if (newReference == null)
                throw new ArgumentNullException(nameof(newReference));

            _target.ExternalReferences.Add(newReference);

            //Set master reference as the new one if it is the only reference in the collection
            var isOnlyReference = _target.ExternalReferences.Count == 1;
            return isOnlyReference ? _target.SetMasterReference(newReference) : newReference;
        }
    }
}
