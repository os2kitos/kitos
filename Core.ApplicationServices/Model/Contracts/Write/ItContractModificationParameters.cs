using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

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
        public Maybe<IEnumerable<ItContractHandoverTrialUpdate>> HandoverTrials { get; set; } = Maybe<IEnumerable<ItContractHandoverTrialUpdate>>.None;
        public Maybe<IEnumerable<Guid>> SystemUsageUuids { get; set; } = Maybe<IEnumerable<Guid>>.None;
    }
}
