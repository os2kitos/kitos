using Core.Abstractions.Types;
using Core.DomainModel;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.Generic.Write
{
    public interface IAssignmentUpdateService
    {
        public Maybe<OperationError> UpdateIndependentOptionTypeAssignment<TDestination, TOption>(
            TDestination destination,
            Guid? optionTypeUuid,
            Action<TDestination> onReset,
            Func<TDestination, TOption> getCurrentValue,
            Action<TDestination, TOption> updateValue)
                where TOption : OptionEntity<TDestination>
                where TDestination : IOwnedByOrganization;

        public Maybe<OperationError> UpdateUniqueMultiAssignment<TDestination, TAssignmentInput, TAssignmentState>(
            string subject,
            TDestination destination,
            Maybe<IEnumerable<Guid>> assignedItemUuids,
            Func<Guid, Result<TAssignmentInput, OperationError>> getAssignmentInputFromInputKey,
            Func<TDestination, IEnumerable<TAssignmentState>> getExistingState,
            Func<TDestination, TAssignmentInput, Maybe<OperationError>> assign,
            Func<TDestination, TAssignmentState, Maybe<OperationError>> unAssign)
                where TAssignmentState : class, IHasId, IHasUuid
                where TAssignmentInput : class, IHasId, IHasUuid;
    }
}
