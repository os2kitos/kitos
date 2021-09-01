using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.Shared;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.GDPR.Write
{
    public class UpdatedDataProcessingRegistrationOversightDataParameters
    {
        public OptionalValueChange<Maybe<IEnumerable<Guid>>> OversightOptionUuids { get; set; } = OptionalValueChange<Maybe<IEnumerable<Guid>>>.None;
        public OptionalValueChange<string> OversightOptionsRemark { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<YearMonthIntervalOption?> OversightInterval { get; set; } = OptionalValueChange<YearMonthIntervalOption?>.None;
        public OptionalValueChange<string> OversightIntervalRemark { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<YesNoUndecidedOption?> IsOversightCompleted { get; set; } = OptionalValueChange<YesNoUndecidedOption?>.None;
        public OptionalValueChange<string> OversightCompletedRemark { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<Maybe<IEnumerable<UpdatedDataProcessingRegistrationOversightDate>>> OversightDates { get; set; } = OptionalValueChange<Maybe<IEnumerable<UpdatedDataProcessingRegistrationOversightDate>>>.None;
    }
}