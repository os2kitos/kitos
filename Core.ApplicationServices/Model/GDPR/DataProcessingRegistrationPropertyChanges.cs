using System;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.Shared;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.GDPR
{
    public class DataProcessingRegistrationPropertyChanges
    {
        public Maybe<ChangedValue<string>> NameChange { get; set; } = Maybe<ChangedValue<string>>.None;
        public Maybe<ChangedValue<YesNoIrrelevantOption?>> IsAgreementConcludedChange { get; set; } = Maybe<ChangedValue<YesNoIrrelevantOption?>>.None;
        public Maybe<ChangedValue<DateTime?>> AgreementConcludedAtChange { get; set; } = Maybe<ChangedValue<DateTime?>>.None;
    }
}
