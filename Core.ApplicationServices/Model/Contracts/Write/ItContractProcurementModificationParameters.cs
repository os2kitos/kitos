using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractProcurementModificationParameters
    {
        public OptionalValueChange<Guid?> PurchaseTypeUuid { get; set; } = OptionalValueChange<Guid?>.None; 
        public OptionalValueChange<Guid?> ProcurementStrategyUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<Maybe<byte>> HalfOfYear { get; set; } = OptionalValueChange<Maybe<byte>>.None;
        public OptionalValueChange<Maybe<int>> Year { get; set; } = OptionalValueChange<Maybe<int>>.None;
    }
}