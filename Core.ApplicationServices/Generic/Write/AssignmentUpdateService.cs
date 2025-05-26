using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.ApplicationServices.Generic.Write
{
    public class AssignmentUpdateService : IAssignmentUpdateService
    {
        private readonly IOptionResolver _optionResolver;

        public AssignmentUpdateService(IOptionResolver optionResolver)
        {
            _optionResolver = optionResolver;
        }

        public Maybe<OperationError> UpdateIndependentOptionTypeAssignment<TDestination, TOption>(
            TDestination destination,
            Guid? optionTypeUuid,
            Action<TDestination> onReset,
            Func<TDestination, TOption> getCurrentValue,
            Action<TDestination, TOption> updateValue)
                where TOption : OptionEntity<TDestination>
                where TDestination : IOwnedByOrganization
        {
            if (optionTypeUuid == null)
            {
                onReset(destination);
            }
            else
            {
                var optionType = _optionResolver.GetOptionType<TDestination, TOption>(destination.Organization.Uuid, optionTypeUuid.Value);
                if (optionType.Failed)
                {
                    return new OperationError($"Failure while resolving {typeof(TOption).Name} option:{optionType.Error.Message.GetValueOrEmptyString()}", optionType.Error.FailureType);
                }

                var option = optionType.Value;
                var currentValue = getCurrentValue(destination);
                if (option.available == false && (currentValue == null || currentValue.Uuid != optionTypeUuid.Value))
                {
                    return new OperationError($"The changed {typeof(TOption).Name} points to an option which is not available in the organization", OperationFailure.BadInput);
                }

                updateValue(destination, option.option);
            }

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> UpdateUniqueMultiAssignment<TDestination, TAssignmentInput, TAssignmentState>(
            string subject,
            TDestination destination,
            Maybe<IEnumerable<Guid>> assignedItemUuids,
            Func<Guid, Result<TAssignmentInput, OperationError>> getAssignmentInputFromKey,
            Func<TDestination, IEnumerable<TAssignmentState>> getExistingState,
            Func<TDestination, TAssignmentInput, Maybe<OperationError>> assign,
            Func<TDestination, TAssignmentState, Maybe<OperationError>> unAssign,
            Func<TDestination, TAssignmentState, Maybe<OperationError>> update = null)
            where TAssignmentState : class, IHasId, IHasUuid
        {
            var newUuids = assignedItemUuids.Match(uuids => uuids.ToList(), () => new List<Guid>());
            if (newUuids.Distinct().Count() != newUuids.Count)
            {
                return new OperationError($"Duplicates of '{subject}' are not allowed", OperationFailure.BadInput);
            }
            var existingAssignments = getExistingState(destination).ToDictionary(x => x.Uuid);
            var itemsToUpdate = update != null ? existingAssignments.ToDictionary(x => x.Key, x => x.Value) : null;
            var existingUuids = existingAssignments.Values.Select(x => x.Uuid).ToList();

            var changes = existingUuids.ComputeDelta(newUuids, uuid => uuid).ToList();
            foreach (var (delta, uuid) in changes)
            {
                switch (delta)
                {
                    case EnumerableExtensions.EnumerableDelta.Added:
                        var assignmentInputResult = getAssignmentInputFromKey(uuid);
                        if (assignmentInputResult.Failed)
                            return new OperationError($"New '{subject}' uuid does not match a KITOS {typeof(TAssignmentInput).Name}: {uuid}", OperationFailure.BadInput);

                        var addResult = assign(destination, assignmentInputResult.Value);

                        if (addResult.HasValue)
                            return new OperationError($"Failed to add during multi assignment with error message: {addResult.Value.Message.GetValueOrEmptyString()}", addResult.Value.FailureType);

                        break;
                    case EnumerableExtensions.EnumerableDelta.Removed:
                        var removeError = unAssign(destination, existingAssignments[uuid]);
                        if (removeError.HasValue)
                            return new OperationError($"Failed to remove during multi assignment with error message: {removeError.Value.Message.GetValueOrEmptyString()}", removeError.Value.FailureType);

                        itemsToUpdate?.Remove(uuid);//Remove state before performing updates
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            //Optionally perform updates
            if (update != null)
            {
                foreach (var assignmentState in itemsToUpdate.Values)
                {
                    //Perform the update to existing items
                    var updateError = update(destination, assignmentState);
                    if (updateError.HasValue)
                        return new OperationError($"Failed to update during multi assignment with error message: {updateError.Value.Message.GetValueOrEmptyString()}", updateError.Value.FailureType);
                }
            }

            return Maybe<OperationError>.None;
        }
    }
}
