using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Core.DomainServices.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.ApplicationServices.Generic.Write
{
    public class UpdateAssignmentHelper : IUpdateAssignmentHelper
    {
        private readonly IOptionResolver _optionResolver;
        private readonly IEntityIdentityResolver _entityIdentityResolver;

        public UpdateAssignmentHelper(IOptionResolver optionResolver, IEntityIdentityResolver entityIdentityResolver)
        {
            _optionResolver = optionResolver;
            _entityIdentityResolver = entityIdentityResolver;
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

        public Maybe<OperationError> UpdateMultiAssignment<TDestination, TAssignment>(
            string subject,
            TDestination destination,
            Maybe<IEnumerable<Guid>> assignedItemUuid,
            Func<TDestination, IEnumerable<TAssignment>> getExistingState,
            Func<TDestination, int, Maybe<OperationError>> assign,
            Func<TDestination, int, Maybe<OperationError>> unAssign)
                where TAssignment : class, IHasId, IHasUuid
                where TDestination : IOwnedByOrganization
        {
            var newUuids = assignedItemUuid.Match(uuids => uuids.ToList(), () => new List<Guid>());
            if (newUuids.Distinct().Count() != newUuids.Count)
            {
                return new OperationError($"Duplicates of '{subject}' are not allowed", OperationFailure.BadInput);
            }
            var existingAssignments = getExistingState(destination).ToDictionary(x => x.Uuid);
            var existingUuids = existingAssignments.Values.Select(x => x.Uuid).ToList();

            var changes = existingUuids.ComputeDelta(newUuids, uuid => uuid).ToList();
            foreach (var (delta, uuid) in changes)
            {
                switch (delta)
                {
                    case EnumerableExtensions.EnumerableDelta.Added:
                        var dbId = _entityIdentityResolver.ResolveDbId<TAssignment>(uuid);

                        if (dbId.IsNone)
                            return new OperationError($"New '{subject}' uuid does not match a KITOS {typeof(TAssignment).Name}: {uuid}", OperationFailure.BadInput);
                        var addResult = assign(destination, dbId.Value);

                        if (addResult.HasValue)
                            return new OperationError($"Failed to add during multi assignment with error message: {addResult.Value.Message.GetValueOrEmptyString()}", addResult.Value.FailureType);

                        break;
                    case EnumerableExtensions.EnumerableDelta.Removed:
                        var removeError = unAssign(destination, existingAssignments[uuid].Id);
                        if (removeError.HasValue)
                            return new OperationError($"Failed to remove during multi assignment with error message: {removeError.Value.Message.GetValueOrEmptyString()}", removeError.Value.FailureType); ;

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return Maybe<OperationError>.None;
        }
    }
}
