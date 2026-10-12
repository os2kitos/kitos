using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.DomainModel.GDPR;

namespace Core.ApplicationServices.Model.GDPR.Write
{
    public class DataProcessingRegistrationModificationParameters: BaseDataProcessingRegistrationParameters, ISupplierAssociatedEntityUpdateParameters
    {
        public IEnumerable<string> GetChangedPropertyKeys(DataProcessingRegistration dpr)
        {
            var changed = new List<string>();

            if (Name.HasChange)
                changed.Add(nameof(Name));

            if (General.HasValue)
            {
                changed.AddRange(GetChangedGeneral());
            }

            if (SystemUsageUuidsHasChange(dpr))
                changed.Add(nameof(SystemUsageUuids));

            if (Oversight.HasValue)
            {
                changed.AddRange(GetChangedOversight());
            }

            if (Roles.HasValue)
            {
                var roles = Roles.Value;
                if (roles.UserRolePairs.HasChange)
                    changed.Add(nameof(roles.UserRolePairs));
            }

            if (ExternalReferencesHasChange(dpr))
                changed.Add(nameof(ExternalReferences));

            return changed;
        }

        private IEnumerable<string> GetChangedGeneral()
        {
            var changed = new List<string>();
            var general = General.Value;
            if (general.DataResponsibleUuid.HasChange)
                changed.Add(nameof(general.DataResponsibleUuid));
            if (general.DataResponsibleRemark.HasChange)
                changed.Add(nameof(general.DataResponsibleRemark));
            if (general.IsAgreementConcluded.HasChange)
                changed.Add(nameof(general.IsAgreementConcluded));
            if (general.IsAgreementConcludedRemark.HasChange)
                changed.Add(nameof(general.IsAgreementConcludedRemark));
            if (general.AgreementConcludedAt.HasChange)
                changed.Add(nameof(general.AgreementConcludedAt));
            if (general.BasisForTransferUuid.HasChange)
                changed.Add(nameof(general.BasisForTransferUuid));
            if (general.TransferToInsecureThirdCountries.HasChange)
                changed.Add(nameof(general.TransferToInsecureThirdCountries));
            if (general.InsecureCountriesSubjectToDataTransferUuids.HasChange)
                changed.Add(nameof(general.InsecureCountriesSubjectToDataTransferUuids));
            if (general.DataProcessorUuids.HasChange)
                changed.Add(nameof(general.DataProcessorUuids));
            if (general.HasSubDataProcessors.HasChange)
                changed.Add(nameof(general.HasSubDataProcessors));
            if (general.SubDataProcessors.HasChange)
                changed.Add(nameof(general.SubDataProcessors));
            if (general.MainContractUuid.HasChange)
                changed.Add(nameof(general.MainContractUuid));
            if (general.ResponsibleUnitUuid.HasChange)
                changed.Add(nameof(general.ResponsibleUnitUuid));
            return changed;
        }

        private IEnumerable<string> GetChangedOversight()
        {
            var changed = new List<string>();
            var oversight = Oversight.Value;
            if (oversight.OversightOptionUuids.HasChange)
                changed.Add(nameof(oversight.OversightOptionUuids));
            if (oversight.OversightOptionsRemark.HasChange)
                changed.Add(nameof(oversight.OversightOptionsRemark));
            if (oversight.OversightInterval.HasChange)
                changed.Add(nameof(oversight.OversightInterval));
            if (oversight.OversightIntervalRemark.HasChange)
                changed.Add(nameof(oversight.OversightIntervalRemark));
            if (oversight.IsOversightCompleted.HasChange)
                changed.Add(nameof(oversight.IsOversightCompleted));
            if (oversight.OversightCompletedRemark.HasChange)
                changed.Add(nameof(oversight.OversightCompletedRemark));
            if (oversight.OversightScheduledInspectionDate.HasChange)
                changed.Add(nameof(oversight.OversightScheduledInspectionDate));
            if (oversight.OversightDates.HasChange)
                changed.Add(nameof(oversight.OversightDates));
            return changed;
        }

        private bool ExternalReferencesHasChange(DataProcessingRegistration dpr)
        {
            var existingReferences = dpr.ExternalReferences;
            if (ExternalReferences.IsNone && existingReferences.IsNullOrEmpty()) return false;
            if (ExternalReferences.HasValue && existingReferences != null)
            {
                var updatedReferences = ExternalReferences.Value.ToList();
                return existingReferences.Count != updatedReferences.Count() || updatedReferences.Any(u =>
                    existingReferences.FirstOrDefault(e => e.Uuid == u.Uuid) == null);
            }
            return true;

        }

        private bool SystemUsageUuidsHasChange(DataProcessingRegistration dpr)
        {
            var existingSystemUsages = dpr.SystemUsages;
            if (SystemUsageUuids.IsNone && existingSystemUsages.IsNullOrEmpty()) return false;
            if (SystemUsageUuids.HasValue && existingSystemUsages != null)
            {
                var existingSystemUsageUuids = existingSystemUsages.Select(su => su.Uuid).ToHashSet();
                var updatedSystemUsageUuidsHashSet = SystemUsageUuids.Value.ToHashSet();
                return !existingSystemUsageUuids.SetEquals(updatedSystemUsageUuidsHashSet);
            }
            return true;
        }
    }
}
