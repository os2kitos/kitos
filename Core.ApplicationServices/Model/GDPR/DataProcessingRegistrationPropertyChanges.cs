using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.Shared;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.GDPR
{
    public class DataProcessingRegistrationPropertyChanges
    {
        public Maybe<ChangedValue<string>> NameChange { get; set; } = Maybe<ChangedValue<string>>.None;

        public Maybe<ChangedValue<YearMonthIntervalOption?>> OversightIntervalChange { get; set; } =
            Maybe<ChangedValue<YearMonthIntervalOption?>>.None;

        public Maybe<ChangedValue<string>> OversightIntervalNoteChange { get; set; } = Maybe<ChangedValue<string>>.None;

    }


}
