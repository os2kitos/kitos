using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class UpdatedSystemUsageOrganizationalUseParameters
    {
        public OptionalValueChange<Maybe<IEnumerable<Guid>>> UsingOrganizationUnitUuids { get; set; } = OptionalValueChange<Maybe<IEnumerable<Guid>>>.None;
        public OptionalValueChange<Maybe<Guid>> ResponsibleOrganizationUnitUuid { get; set; } = OptionalValueChange<Maybe<Guid>>.None;
    }
}
