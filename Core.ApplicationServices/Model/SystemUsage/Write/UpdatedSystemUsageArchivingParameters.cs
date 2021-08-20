using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class UpdatedSystemUsageArchivingParameters
    {
        public Maybe<ChangedValue<ArchiveDutyTypes?>> ArchiveDuty { get; set; } = Maybe<ChangedValue<ArchiveDutyTypes?>>.None;
        public Maybe<ChangedValue<Maybe<Guid>>> ArchiveTypeUuid { get; set; } = Maybe<ChangedValue<Maybe<Guid>>>.None;
        public Maybe<ChangedValue<Maybe<Guid>>> ArchiveLocationUuid { get; set; } = Maybe<ChangedValue<Maybe<Guid>>>.None;
        public Maybe<ChangedValue<Maybe<Guid>>> ArchiveTestLocationUuid { get; set; } = Maybe<ChangedValue<Maybe<Guid>>>.None;
        public Maybe<ChangedValue<Maybe<Guid>>> ArchiveSupplierOrganizationUuid { get; set; } = Maybe<ChangedValue<Maybe<Guid>>>.None;
        public Maybe<ChangedValue<Maybe<bool>>> ArchiveActive { get; set; } = Maybe<ChangedValue<Maybe<bool>>>.None;
        public Maybe<ChangedValue<string>> ArchiveNotes { get; set; } = Maybe<ChangedValue<string>>.None;
        public Maybe<ChangedValue<Maybe<int>>> ArchiveFrequencyInMonths { get; set; } = Maybe<ChangedValue<Maybe<int>>>.None;
        public Maybe<ChangedValue<Maybe<bool>>> ArchiveDocumentBearing { get; set; } = Maybe<ChangedValue<Maybe<bool>>>.None;
        public Maybe<ChangedValue<Maybe<IEnumerable<SystemUsageJournalPeriod>>>> ArchiveJournalPeriods = Maybe<ChangedValue<Maybe<IEnumerable<SystemUsageJournalPeriod>>>>.None;
    }
}
