using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.GDPR.Write
{
    public class UpdatedDataProcessingRegistrationOversightDateParameters: ISupplierAssociatedEntityUpdateParameters
    {
        public OptionalValueChange<DateTime?> CompletedAt { get; set; } = OptionalValueChange<DateTime?>.None;
        public OptionalValueChange<string> Remark { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> OversightReportLink { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> OversightReportLinkName { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<Guid?> OversightOptionUuid { get; set; } = OptionalValueChange<Guid?>.None;

        public IEnumerable<string> GetChangedPropertyKeys()
        {
            var changedProperties = new List<string>();

            if(CompletedAt.HasChange)
                changedProperties.Add(nameof(CompletedAt));
            if(Remark.HasChange)
                changedProperties.Add(nameof(Remark));
            if(OversightReportLink.HasChange)
                changedProperties.Add(nameof(OversightReportLink));
            if(OversightReportLinkName.HasChange)
                changedProperties.Add(nameof(OversightReportLinkName));
            if(OversightOptionUuid.HasChange)
                changedProperties.Add(nameof(OversightOptionUuid));
            return changedProperties;
        }
    }
}
