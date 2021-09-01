using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class UpdatedSystemUsageArchivingParameters
    {
        public OptionalValueChange<ArchiveDutyTypes?> ArchiveDuty { get; set; } = OptionalValueChange<ArchiveDutyTypes?>.None;
        public OptionalValueChange<Maybe<Guid>> ArchiveTypeUuid { get; set; } = OptionalValueChange<Maybe<Guid>>.None;
        public OptionalValueChange<Maybe<Guid>> ArchiveLocationUuid { get; set; } = OptionalValueChange<Maybe<Guid>>.None;
        public OptionalValueChange<Maybe<Guid>> ArchiveTestLocationUuid { get; set; } = OptionalValueChange<Maybe<Guid>>.None;
        public OptionalValueChange<Maybe<Guid>> ArchiveSupplierOrganizationUuid { get; set; } = OptionalValueChange<Maybe<Guid>>.None;
        public OptionalValueChange<bool?> ArchiveActive { get; set; } = OptionalValueChange<bool?>.None;
        public OptionalValueChange<string> ArchiveNotes { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<int?> ArchiveFrequencyInMonths { get; set; } = OptionalValueChange<int?>.None;
        public OptionalValueChange<bool?> ArchiveDocumentBearing { get; set; } = OptionalValueChange<bool?>.None;
        public OptionalValueChange<Maybe<IEnumerable<SystemUsageJournalPeriod>>> ArchiveJournalPeriods = OptionalValueChange<Maybe<IEnumerable<SystemUsageJournalPeriod>>>.None;
    }
}
