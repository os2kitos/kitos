using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class UpdatedSystemUsageOrganizationalUseParameters
    {
        public Maybe<ChangedValue<Maybe<IEnumerable<Guid>>>> UsingOrganizationUnitUuids { get; set; } = Maybe<ChangedValue<Maybe<IEnumerable<Guid>>>>.None;
        public Maybe<ChangedValue<Maybe<Guid>>> ResponsibleOrganizationUnitUuid { get; set; } = Maybe<ChangedValue<Maybe<Guid>>>.None;
    }
}
