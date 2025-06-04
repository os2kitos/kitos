using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.Shared;


namespace Core.ApplicationServices.Model.GDPR.Write
{
    public class UpdatedDataProcessingRegistrationGeneralDataParameters
    {
        public OptionalValueChange<Guid?> DataResponsibleUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<string> DataResponsibleRemark { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<YesNoIrrelevantOption?> IsAgreementConcluded { get; set; } = OptionalValueChange<YesNoIrrelevantOption?>.None;
        public OptionalValueChange<string> IsAgreementConcludedRemark { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<DateTime?> AgreementConcludedAt { get; set; } = OptionalValueChange<DateTime?>.None;
        public OptionalValueChange<Guid?> BasisForTransferUuid { get; set; } = OptionalValueChange<Guid?>.None;
        public OptionalValueChange<YesNoUndecidedOption?> TransferToInsecureThirdCountries { get; set; } = OptionalValueChange<YesNoUndecidedOption?>.None;
        public OptionalValueChange<Maybe<IEnumerable<Guid>>> InsecureCountriesSubjectToDataTransferUuids { get; set; } = OptionalValueChange<Maybe<IEnumerable<Guid>>>.None;
        public OptionalValueChange<Maybe<IEnumerable<Guid>>> DataProcessorUuids { get; set; } = OptionalValueChange<Maybe<IEnumerable<Guid>>>.None;
        public OptionalValueChange<YesNoUndecidedOption?> HasSubDataProcessors { get; set; } = OptionalValueChange<YesNoUndecidedOption?>.None;
        public OptionalValueChange<Maybe<IEnumerable<SubDataProcessorParameter>>> SubDataProcessors { get; set; } = OptionalValueChange<Maybe<IEnumerable<SubDataProcessorParameter>>>.None;
        public OptionalValueChange<Guid?> MainContractUuid { get; set; } = OptionalValueChange<Guid?>.None;

        public OptionalValueChange<Guid?> ResponsibleUnitUuid { get; set; } = OptionalValueChange<Guid?>.None;
    }
}
