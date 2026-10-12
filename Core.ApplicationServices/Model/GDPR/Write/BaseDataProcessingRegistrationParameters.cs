using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.Model.GDPR.Write
{
    public abstract class BaseDataProcessingRegistrationParameters
    {
        public OptionalValueChange<string> Name = OptionalValueChange<string>.None;
        public Maybe<UpdatedDataProcessingRegistrationGeneralDataParameters> General { get; set; } = Maybe<UpdatedDataProcessingRegistrationGeneralDataParameters>.None;
        public Maybe<IEnumerable<Guid>> SystemUsageUuids { get; set; } = Maybe<IEnumerable<Guid>>.None;
        public Maybe<UpdatedDataProcessingRegistrationOversightDataParameters> Oversight { get; set; } = Maybe<UpdatedDataProcessingRegistrationOversightDataParameters>.None;
        public Maybe<UpdatedDataProcessingRegistrationRoles> Roles { get; set; } = Maybe<UpdatedDataProcessingRegistrationRoles>.None;
        public Maybe<IEnumerable<UpdatedExternalReferenceProperties>> ExternalReferences { get; set; } = Maybe<IEnumerable<UpdatedExternalReferenceProperties>>.None;
    }
}
