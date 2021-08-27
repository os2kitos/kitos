using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.GDPR.Write
{
    public class DataProcessingRegistrationModificationParameters
    {
        public OptionalValueChange<string> Name = OptionalValueChange<string>.None;
    }
}
