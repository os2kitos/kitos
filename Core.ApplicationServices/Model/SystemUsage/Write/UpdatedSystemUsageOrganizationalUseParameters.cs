using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;


namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class UpdatedSystemUsageOrganizationalUseParameters
    {
        public OptionalValueChange<Maybe<IEnumerable<Guid>>> UsingOrganizationUnitUuids { get; set; } = OptionalValueChange<Maybe<IEnumerable<Guid>>>.None;
        public OptionalValueChange<Maybe<Guid>> ResponsibleOrganizationUnitUuid { get; set; } = OptionalValueChange<Maybe<Guid>>.None;
    }
}
