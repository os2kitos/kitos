using System;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractResponsibleDataModificationParameters
    {
        public OptionalValueChange<Guid?> OrganizationUnitUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<string> SignedBy { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<bool> Signed { get; set; } = OptionalValueChange<bool>.None;
        public OptionalValueChange<DateTime?> SignedAt { get; set; } = OptionalValueChange<DateTime?>.None;
    }
}
