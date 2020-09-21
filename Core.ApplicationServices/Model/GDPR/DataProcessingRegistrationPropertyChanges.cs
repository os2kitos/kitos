using Core.ApplicationServices.Model.Shared;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.GDPR
{
    public class DataProcessingRegistrationPropertyChanges
    {
        public Maybe<ChangedValue<string>> NameChange { get; set; } = Maybe<ChangedValue<string>>.None;
    }
}
