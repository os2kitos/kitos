using System;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractSupplierModificationParameters
    {
        public OptionalValueChange<Guid?> OrganizationUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<string> SignedBy { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<bool> Signed { get; set; } = OptionalValueChange<bool>.None;
        public OptionalValueChange<DateTime?> SignedAt { get; set; } = OptionalValueChange<DateTime?>.None;
    }
}
