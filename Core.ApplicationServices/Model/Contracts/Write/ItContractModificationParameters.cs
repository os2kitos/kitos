using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractModificationParameters
    {
        public OptionalValueChange<string> Name { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<Guid?> ParentContractUuid { get; set; } = OptionalValueChange<Guid?>.None;

        public Maybe<ItContractGeneralDataModificationParameters> General { get; set; } = Maybe<ItContractGeneralDataModificationParameters>.None;
        public Maybe<ItContractProcurementModificationParameters> Procurement { get; set; } = Maybe<ItContractProcurementModificationParameters>.None;
        public Maybe<ItContractResponsibleDataModificationParameters> Responsible { get; set; } = Maybe<ItContractResponsibleDataModificationParameters>.None;
        public Maybe<ItContractSupplierModificationParameters> Supplier { get; set; } = Maybe<ItContractSupplierModificationParameters>.None;
        public Maybe<IEnumerable<UpdatedExternalReferenceProperties>> ExternalReferences { get; set; } = Maybe<IEnumerable<UpdatedExternalReferenceProperties>>.None;
        public Maybe<IEnumerable<Guid>> SystemUsageUuids { get; set; } = Maybe<IEnumerable<Guid>>.None;
        public Maybe<IEnumerable<UserRolePair>> Roles { get; set; } = Maybe<IEnumerable<UserRolePair>>.None;
        public Maybe<IEnumerable<Guid>> DataProcessingRegistrationUuids { get; set; } = Maybe<IEnumerable<Guid>>.None;
        public Maybe<ItContractAgreementPeriodModificationParameters> AgreementPeriod { get; set; } = Maybe<ItContractAgreementPeriodModificationParameters>.None;
        public Maybe<ItContractPaymentModelModificationParameters> PaymentModel { get; set; } = Maybe<ItContractPaymentModelModificationParameters>.None;
        public Maybe<ItContractPaymentDataModificationParameters> Payments { get; set; } = Maybe<ItContractPaymentDataModificationParameters>.None;
        public Maybe<ItContractTerminationParameters> Termination { get; set; } = Maybe<ItContractTerminationParameters>.None;
    }
}
