﻿using Core.Abstractions.Types;
using Core.DomainModel;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.Generic.Write
{
    public interface IUpdateAssignmentHelper
    {
        public Maybe<OperationError> UpdateIndependentOptionTypeAssignment<TDestination, TOption>(
            TDestination destination,
            Guid? optionTypeUuid,
            Action<TDestination> onReset,
            Func<TDestination, TOption> getCurrentValue,
            Action<TDestination, TOption> updateValue)
                where TOption : OptionEntity<TDestination>
                where TDestination : IOwnedByOrganization;

        public Maybe<OperationError> UpdateMultiAssignment<TDestination, TAssignment>(
            string subject,
            TDestination destination,
            Maybe<IEnumerable<Guid>> assignedItemUuid,
            Func<TDestination, IEnumerable<TAssignment>> getExistingState,
            Func<TDestination, int, Maybe<OperationError>> assign,
            Func<TDestination, int, Maybe<OperationError>> unAssign)
                where TAssignment : class, IHasId, IHasUuid
                where TDestination : IOwnedByOrganization;
    }
}
