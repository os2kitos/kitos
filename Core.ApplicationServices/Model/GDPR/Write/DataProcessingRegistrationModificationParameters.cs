using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.GDPR.Write
{
    public class DataProcessingRegistrationModificationParameters
    {
        public OptionalValueChange<string> Name = OptionalValueChange<string>.None;
        public Maybe<UpdatedDataProcessingRegistrationGeneralDataParameters> General { get; set; } = Maybe<UpdatedDataProcessingRegistrationGeneralDataParameters>.None;
        public Maybe<IEnumerable<Guid>> SystemUsageUuids { get; set; } = Maybe<IEnumerable<Guid>>.None;
    }
}
