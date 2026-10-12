using System;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractSupplierModificationParameters
    {
        public OptionalValueChange<Guid?> OrganizationUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<Guid?> OrganizationUnitUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<bool> IsInternal { get; set; } = OptionalValueChange<bool>.None;
        public OptionalValueChange<string> SignedBy { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<bool> Signed { get; set; } = OptionalValueChange<bool>.None;
        public OptionalValueChange<DateTime?> SignedAt { get; set; } = OptionalValueChange<DateTime?>.None;
        public OptionalValueChange<string> ContactPerson { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<bool> UseSignedByForContact { get; set; } = OptionalValueChange<bool>.None;
        public OptionalValueChange<string> ContactPhoneNumber { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> ContactEmail { get; set; } = OptionalValueChange<string>.None;
    }
}
