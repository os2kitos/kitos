using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.ItContract;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractTerminationParameters
    {
        public OptionalValueChange<Maybe<DateTime>> TerminatedAt { get; set; } = OptionalValueChange<Maybe<DateTime>>.None;
        public OptionalValueChange<Guid?> NoticePeriodMonthsUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<Maybe<YearSegmentOption>> NoticePeriodExtendsCurrent { get; set; } = OptionalValueChange<Maybe<YearSegmentOption>>.None;
        public OptionalValueChange<Maybe<YearSegmentOption>> NoticeByEndOf { get; set; } = OptionalValueChange<Maybe<YearSegmentOption>>.None;
    }
}