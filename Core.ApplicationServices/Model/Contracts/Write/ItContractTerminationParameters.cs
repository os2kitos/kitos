using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractTerminationParameters
    {
        public OptionalValueChange<Maybe<DateTime>> TerminatedAt { get; set; } = OptionalValueChange<Maybe<DateTime>>.None;

        public OptionalValueChange<Maybe<ItContractTerminationTerms>> TerminationTerms { get; set; } = OptionalValueChange<Maybe<ItContractTerminationTerms>>.None;
    }
}