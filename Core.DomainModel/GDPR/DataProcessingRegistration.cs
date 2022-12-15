using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.Extensions;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.ItContract;
using Core.DomainModel.Notification;
using Core.DomainModel.References;
using Core.DomainModel.Shared;



namespace Core.DomainModel.GDPR
{
    public class DataProcessingRegistration :
        HasRightsEntity<DataProcessingRegistration, DataProcessingRegistrationRight, DataProcessingRegistrationRole>,
        IHasName,
        IDataProcessingModule,
        IEntityWithExternalReferences,
        IEntityWithAdvices,
        IEntityWithUserNotification,
        IHasUuid,
        IHasDirtyMarking
    {
        public DataProcessingRegistration()
        {
            ExternalReferences = new List<ExternalReference>();
            SystemUsages = new List<ItSystemUsage.ItSystemUsage>();
            DataProcessors = new List<Organization.Organization>();
            SubDataProcessors = new List<Organization.Organization>();
            InsecureCountriesSubjectToDataTransfer = new List<DataProcessingCountryOption>();
            OversightOptions = new List<DataProcessingOversightOption>();
            AssociatedContracts = new List<ItContract.ItContract>();
            OversightDates = new List<DataProcessingRegistrationOversightDate>();
            UserNotifications = new List<UserNotification>();
            Uuid = Guid.NewGuid();
            MarkAsDirty();
        }

        public Guid Uuid { get; set; }

        public static bool IsNameValid(string name) => !string.IsNullOrWhiteSpace(name) &&
                                                       name.Length <= DataProcessingRegistrationConstraints.MaxNameLength;

        public Maybe<OperationError> SetName(string newName)
        {
            if (IsNameValid(newName))
            {
                Name = newName;
                return Maybe<OperationError>.None;
            }
            return new OperationError("Name does not meet validation criteria", OperationFailure.BadInput);
        }

        public string Name { get; set; }

        public int OrganizationId { get; set; }

        public YesNoUndecidedOption? HasSubDataProcessors { get; set; }

        public void SetHasSubDataProcessors(YesNoUndecidedOption hasSubDataProcessors)
        {
            HasSubDataProcessors = hasSubDataProcessors;
            if (hasSubDataProcessors != YesNoUndecidedOption.Yes)
            {
                SubDataProcessors.Clear();
            }
        }

        public YesNoUndecidedOption? TransferToInsecureThirdCountries { get; set; }

        public void SetTransferToInsecureThirdCountries(YesNoUndecidedOption transferToInsecureThirdCountries)
        {
            TransferToInsecureThirdCountries = transferToInsecureThirdCountries;
            if (transferToInsecureThirdCountries != YesNoUndecidedOption.Yes)
            {
                InsecureCountriesSubjectToDataTransfer.Clear();
            }
        }

        public virtual ICollection<DataProcessingCountryOption> InsecureCountriesSubjectToDataTransfer { get; set; }

        public virtual Organization.Organization Organization { get; set; }

        public virtual ICollection<DataProcessingRegistrationReadModel> ReadModels { get; set; }

        public virtual ICollection<ItSystemUsage.ItSystemUsage> SystemUsages { get; set; }

        public virtual ICollection<Organization.Organization> DataProcessors { get; set; }

        public virtual ICollection<Organization.Organization> SubDataProcessors { get; set; }

        public virtual DataProcessingDataResponsibleOption DataResponsible { get; set; }
        public int? DataResponsible_Id { get; set; }

        public string DataResponsibleRemark { get; set; }

        public virtual ICollection<DataProcessingOversightOption> OversightOptions { get; set; }
        public string OversightOptionRemark { get; set; }

        public Result<Organization.Organization, OperationError> AssignDataProcessor(Organization.Organization dataProcessor)
        {
            if (dataProcessor == null) throw new ArgumentNullException(nameof(dataProcessor));
            if (HasDataProcessor(dataProcessor))
                return new OperationError("Data processor already assigned", OperationFailure.Conflict);

            DataProcessors.Add(dataProcessor);

            return dataProcessor;
        }

        public Result<Organization.Organization, OperationError> RemoveDataProcessor(Organization.Organization dataProcessor)
        {
            if (dataProcessor == null) throw new ArgumentNullException(nameof(dataProcessor));
            if (!HasDataProcessor(dataProcessor))
                return new OperationError("Data processor not assigned", OperationFailure.BadInput);

            DataProcessors.Remove(dataProcessor);

            return dataProcessor;
        }

        public Result<Organization.Organization, OperationError> AssignSubDataProcessor(Organization.Organization dataProcessor)
        {
            if (dataProcessor == null) throw new ArgumentNullException(nameof(dataProcessor));

            if (HasSubDataProcessors != YesNoUndecidedOption.Yes)
                return new OperationError("To Add new sub data processors, enable sub data processors", OperationFailure.BadInput);

            if (HasSubDataProcessor(dataProcessor))
                return new OperationError("Sub Data processor already assigned", OperationFailure.Conflict);

            SubDataProcessors.Add(dataProcessor);

            return dataProcessor;
        }

        public Result<Organization.Organization, OperationError> RemoveSubDataProcessor(Organization.Organization dataProcessor)
        {
            if (dataProcessor == null) throw new ArgumentNullException(nameof(dataProcessor));
            if (!HasSubDataProcessor(dataProcessor))
                return new OperationError("Sub Data processor not assigned", OperationFailure.BadInput);

            SubDataProcessors.Remove(dataProcessor);

            return dataProcessor;
        }

        public Result<DataProcessingCountryOption, OperationError> AssignInsecureCountrySubjectToDataTransfer(DataProcessingCountryOption country)
        {
            if (country == null) throw new ArgumentNullException(nameof(country));

            if (TransferToInsecureThirdCountries != YesNoUndecidedOption.Yes)
                return new OperationError("To Add new insecure data transfer country, but transfer to insecure countries is not enabled", OperationFailure.BadInput);

            if (HasInsecureCountry(country))
                return new OperationError("Country already assigned", OperationFailure.Conflict);

            InsecureCountriesSubjectToDataTransfer.Add(country);

            return country;
        }

        public Result<DataProcessingCountryOption, OperationError> RemoveInsecureCountrySubjectToDataTransfer(DataProcessingCountryOption country)
        {
            if (country == null) throw new ArgumentNullException(nameof(country));

            if (!HasInsecureCountry(country))
                return new OperationError("Country not assigned", OperationFailure.Conflict);

            InsecureCountriesSubjectToDataTransfer.Remove(country);

            return country;
        }

        public Result<DataProcessingOversightOption, OperationError> AssignOversightOption(DataProcessingOversightOption oversightOption)
        {
            if (oversightOption == null) throw new ArgumentNullException(nameof(oversightOption));
            if (HasOversightOption(oversightOption))
                return new OperationError("Oversight option already assigned", OperationFailure.Conflict);

            OversightOptions.Add(oversightOption);

            return oversightOption;
        }

        public Result<DataProcessingOversightOption, OperationError> RemoveOversightOption(DataProcessingOversightOption oversightOption)
        {
            if (oversightOption == null) throw new ArgumentNullException(nameof(oversightOption));
            if (!HasOversightOption(oversightOption))
                return new OperationError("Oversight option not assigned", OperationFailure.BadInput);

            OversightOptions.Remove(oversightOption);

            return oversightOption;
        }

        private bool HasInsecureCountry(DataProcessingCountryOption country)
        {
            return InsecureCountriesSubjectToDataTransfer.Any(c => c.Id == country.Id);
        }

        private bool HasSubDataProcessor(Organization.Organization dataProcessor)
        {
            return SubDataProcessors.Any(x => x.Id == dataProcessor.Id);
        }

        private bool HasDataProcessor(Organization.Organization dataProcessor)
        {
            return DataProcessors.Any(x => x.Id == dataProcessor.Id);
        }

        private bool HasOversightOption(DataProcessingOversightOption oversightOption)
        {
            return OversightOptions.Any(x => x.Id == oversightOption.Id);
        }

        public Result<ItSystemUsage.ItSystemUsage, OperationError> AssignSystem(ItSystemUsage.ItSystemUsage systemUsage)
        {
            if (systemUsage == null) throw new ArgumentNullException(nameof(systemUsage));

            if (GetAssignedSystemUsage(systemUsage.Id).HasValue)
                return new OperationError("System usage is already assigned", OperationFailure.Conflict);

            if (OrganizationId != systemUsage.OrganizationId)
                return new OperationError("System usage must be in the same organization as this data processing registration", OperationFailure.BadInput);

            SystemUsages.Add(systemUsage);

            return systemUsage;
        }

        public Result<ItSystemUsage.ItSystemUsage, OperationError> RemoveSystem(ItSystemUsage.ItSystemUsage systemUsage)
        {
            if (systemUsage == null) throw new ArgumentNullException(nameof(systemUsage));

            if (GetAssignedSystemUsage(systemUsage.Id).IsNone)
                return new OperationError("Usage not assigned", OperationFailure.BadInput);

            SystemUsages.Remove(systemUsage);

            return systemUsage;
        }

        private Maybe<ItSystemUsage.ItSystemUsage> GetAssignedSystemUsage(int usageId)
        {
            return SystemUsages.FirstOrDefault(x => x.Id == usageId).FromNullable();
        }

        public override DataProcessingRegistrationRight CreateNewRight(DataProcessingRegistrationRole role, User user)
        {
            return new DataProcessingRegistrationRight()
            {
                Role = role,
                User = user,
                Object = this
            };
        }

        public virtual ICollection<UserNotification> UserNotifications { get; set; }

        public virtual ICollection<ExternalReference> ExternalReferences { get; set; }

        public ReferenceRootType GetRootType() => ReferenceRootType.DataProcessingRegistration;

        public Result<ExternalReference, OperationError> AddExternalReference(ExternalReference newReference)
        {
            return new AddReferenceCommand(this).AddExternalReference(newReference);
        }

        public void ClearMasterReference()
        {
            Reference.Track();
            Reference = null;
        }

        public Result<ExternalReference, OperationError> SetMasterReference(ExternalReference newReference)
        {
            if (ExternalReferences.Any(x => x.Id == newReference.Id))
            {
                Reference = newReference;
                return newReference;
            }
            return new OperationError("Reference is not known to this object", OperationFailure.BadInput);
        }

        public int? ReferenceId { get; set; }

        public virtual ExternalReference Reference { get; set; }

        public YesNoIrrelevantOption? IsAgreementConcluded { get; set; }

        public DateTime? AgreementConcludedAt { get; set; }

        public string AgreementConcludedRemark { get; set; }

        public int? BasisForTransferId { get; set; }

        public virtual DataProcessingBasisForTransferOption BasisForTransfer { get; set; }

        public YearMonthIntervalOption? OversightInterval { get; set; }

        public string OversightIntervalRemark { get; set; }

        public void SetIsAgreementConcluded(YesNoIrrelevantOption concluded)
        {
            IsAgreementConcluded = concluded;
            if (IsAgreementConcluded != YesNoIrrelevantOption.YES)
            {
                AgreementConcludedAt = null;
            }
        }

        public YesNoUndecidedOption? IsOversightCompleted { get; set; }

        public string OversightCompletedRemark { get; set; }

        public DateTime? OversightScheduledInspectionDate { get; set; }

        public void SetOversightScheduledInspectionDate(DateTime? oversightScheduledInspectionDate)
        {
            OversightScheduledInspectionDate = oversightScheduledInspectionDate;
        }

        public virtual ICollection<DataProcessingRegistrationOversightDate> OversightDates { get; set; }


        public Maybe<IEnumerable<DataProcessingRegistrationOversightDate>> SetOversightCompleted(YesNoUndecidedOption completed)
        {
            IsOversightCompleted = completed;
            if (IsOversightCompleted != YesNoUndecidedOption.Yes)
            {
                var oversightDatesToBeRemoved = OversightDates.ToList();
                OversightDates.Clear();
                return Maybe<IEnumerable<DataProcessingRegistrationOversightDate>>.Some(oversightDatesToBeRemoved);
            }
            return Maybe<IEnumerable<DataProcessingRegistrationOversightDate>>.None;
        }

        public virtual ICollection<ItContract.ItContract> AssociatedContracts { get; set; }
        public int? MainContractId { get; set; }
        public virtual ItContract.ItContract MainContract { get; set; }
        public bool IsActiveAccordingToMainContract => CheckContractValidity();

        public void ResetMainContract()
        {
            MainContract?.Track();
            MainContract = null;
        }

        public Maybe<OperationError> AssignMainContract(int contractId)
        {
            if (MainContract != null && contractId == MainContract.Id)
                return Maybe<OperationError>.None;

            var contract = AssociatedContracts.FirstOrDefault(c => c.Id == contractId);
            if (contract == null)
                return new OperationError($"Contract with id: {contractId} is not associated with this data processing registration", OperationFailure.NotFound);

            MainContract = contract;

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> RemoveMainContract(int contractId)
        {
            if (MainContract == null)
                return Maybe<OperationError>.None;

            if(MainContract.Id != contractId)
                return new OperationError($"Contract with id: {contractId} is not set as the main contract for this data processing registration", OperationFailure.BadInput);

            ResetMainContract();

            return Maybe<OperationError>.None;
        }

        private bool CheckContractValidity()
        {
            return MainContract?.IsActive == false;
        }
        public Result<DataProcessingRegistrationOversightDate, OperationError> AssignOversightDate(DateTime oversightDate, string oversightRemark)
        {
            if (IsOversightCompleted != YesNoUndecidedOption.Yes)
                return new OperationError("Cannot assign oversight dates if 'IsOversightCompleted' is not set to 'Yes'", OperationFailure.BadState);
            if (oversightDate == null) throw new ArgumentNullException(nameof(oversightDate));
            if (oversightRemark == null) throw new ArgumentNullException(nameof(oversightRemark));

            var newOversightDate = new DataProcessingRegistrationOversightDate
            {
                OversightDate = oversightDate,
                OversightRemark = oversightRemark,
                Parent = this
            };

            OversightDates.Add(newOversightDate);

            return newOversightDate;
        }

        public Result<DataProcessingRegistrationOversightDate, OperationError> ModifyOversightDate(int oversightId, DateTime oversightDate, string oversightRemark)
        {
            if (oversightDate == null) throw new ArgumentNullException(nameof(oversightDate));
            if (oversightRemark == null) throw new ArgumentNullException(nameof(oversightRemark));
            var existingDate = GetOversightDate(oversightId);
            if (existingDate.IsNone)
                return new OperationError("Oversight date not assigned", OperationFailure.BadInput);

            var oversightDateToModify = existingDate.Value;

            oversightDateToModify.OversightDate = oversightDate;
            oversightDateToModify.OversightRemark = oversightRemark;

            return oversightDateToModify;
        }

        public Result<DataProcessingRegistrationOversightDate, OperationError> RemoveOversightDate(int oversightId)
        {
            var oversightDate = GetOversightDate(oversightId);
            if (oversightDate.IsNone)
                return new OperationError("Oversight date not assigned", OperationFailure.BadInput);

            OversightDates.Remove(oversightDate.Value);

            return oversightDate.Value;
        }

        private Maybe<DataProcessingRegistrationOversightDate> GetOversightDate(int oversightId)
        {
            return OversightDates.FirstOrDefault(x => x.Id == oversightId).FromNullable();
        }

        public void MarkAsDirty()
        {
            LastChanged = DateTime.UtcNow;
        }
    }
}
