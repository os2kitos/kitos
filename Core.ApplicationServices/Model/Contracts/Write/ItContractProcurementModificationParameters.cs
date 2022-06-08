using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractProcurementModificationParameters
    {
        public OptionalValueChange<Guid?> PurchaseTypeUuid { get; set; } = OptionalValueChange<Guid?>.None; 
        public OptionalValueChange<Guid?> ProcurementStrategyUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<Maybe<(byte quarter, int year)>> ProcurementPlan { get; set; } = OptionalValueChange<Maybe<(byte quarter, int year)>>.None;
    }
}