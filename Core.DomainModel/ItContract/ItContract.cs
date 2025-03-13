using System;
using System.Collections.Generic;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainModel.GDPR;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.Extensions;
using Core.DomainModel.ItContract.Read;
using Core.DomainModel.Notification;
using Core.DomainModel.Shared;


// ReSharper disable VirtualMemberCallInConstructor

namespace Core.DomainModel.ItContract
{
    /// <summary>
    ///     Contains info about an it contract
    /// </summary>
    public class ItContract : HasRightsEntity<ItContract, ItContractRight, ItContractRole>, IHasReferences, IHierarchy<ItContract>, IContractModule, IOwnedByOrganization, IHasName, IEntityWithExternalReferences, IEntityWithAdvices, IEntityWithUserNotification, IHasUuid, IHasDirtyMarking
    {
        public ItContract()
        {
            Children = new List<ItContract>();
            AssociatedAgreementElementTypes = new List<ItContractAgreementElementTypes>();
            AssociatedSystemUsages = new List<ItContractItSystemUsage>();
            InternEconomyStreams = new List<EconomyStream>();
            ExternEconomyStreams = new List<EconomyStream>();
            ExternalReferences = new List<ExternalReference>();
            DataProcessingRegistrations = new List<DataProcessingRegistration>();
            UserNotifications = new List<UserNotification>();
            Uuid = Guid.NewGuid();
            MarkAsDirty();
            AssociatedSystemRelations = new List<SystemRelation>();
            DataProcessingRegistrationsWhereContractIsMainContract = new List<DataProcessingRegistration>();
        }

        public Guid Uuid { get; set; }

        public ItContractValidationResult Validate(DateTime todayReference)
        {
            var enforcedActive = Active;
            var errors = new List<ItContractValidationError>();

            var today = todayReference.Date;
            var startDate = (Concluded ?? today).Date;
            var endDate = DateTime.MaxValue;

            if (ExpirationDate.HasValue && ExpirationDate.Value.Date != DateTime.MaxValue.Date)
            {
                endDate = ExpirationDate.Value.Date;
            }

            //Valid yet?
            if (today < startDate)
            {
                errors.Add(ItContractValidationError.StartDateNotPassed);
            }
            //Expired?
            if (today > endDate)
            {
                errors.Add(ItContractValidationError.EndDatePassed);
            }
            //If contract has been terminated, determine if the termination deadline has passed
            if (Terminated.HasValue)
            {
                var terminationDate = Terminated.Value.Date;
                if (TerminationDeadline != null)
                {
                    int.TryParse(TerminationDeadline.Name, out var deadline);
                    terminationDate = terminationDate.AddMonths(deadline);
                }
                if (today > terminationDate.Date)
                {
                    errors.Add(ItContractValidationError.TerminationPeriodExceeded);
                }
            }

            if (RequireValidParent && Parent != null && !Parent.IsActive)
            {
                errors.Add(ItContractValidationError.InvalidParentContract);
            }

            return new ItContractValidationResult(enforcedActive, errors);
        }
        public ItContractValidationResult Validate()
        {
            return Validate(DateTime.Now);
        }

        /// <summary>
        ///     Whether the contract is active or not
        /// </summary>
        public bool IsActive => Validate().Result;

        public int? ReferenceId { get; set; }

        public virtual ExternalReference Reference { get; set; }

        public virtual ICollection<UserNotification> UserNotifications { get; set; }

        #region Master

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets Active.
        /// </summary>
        /// <value>
        ///   Active.
        /// </value>
        public bool Active { get; set; }

        /// <summary>
        ///     Gets or sets the note.
        /// </summary>
        /// <value>
        ///     The note.
        /// </value>
        public string Note { get; set; }

        /// <summary>
        ///     Gets or sets it contract identifier defined by the user.
        /// </summary>
        /// <remarks>
        ///     This identifier is NOT the primary key.
        /// </remarks>
        /// <value>
        ///     User defined it contract identifier.
        /// </value>
        public string ItContractId { get; set; }

        /// <summary>
        ///     Gets or sets the supplier contract signer.
        /// </summary>
        /// <value>
        ///     The supplier contract signer.
        /// </value>
        public string SupplierContractSigner { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance has supplier signed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has supplier signed; otherwise, <c>false</c>.
        /// </value>
        public bool HasSupplierSigned { get; set; }

        /// <summary>
        ///     Gets or sets the supplier signed date.
        /// </summary>
        /// <value>
        ///     The supplier signed date.
        /// </value>
        public DateTime? SupplierSignedDate { get; set; }

        /// <summary>
        ///     Gets or sets the contract signer.
        /// </summary>
        /// <value>
        ///     The contract signer.
        /// </value>
        public virtual string ContractSigner { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this contract is signed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if contract is signed; otherwise, <c>false</c>.
        /// </value>
        public bool IsSigned { get; set; }

        /// <summary>
        ///     Gets or sets the signed date.
        /// </summary>
        /// <value>
        ///     The signed date.
        /// </value>
        public DateTime? SignedDate { get; set; }

        /// <summary>
        ///     The chosen responsible org unit for this contract
        /// </summary>
        /// <value>
        ///     The responsible organization unit identifier.
        /// </value>
        public int? ResponsibleOrganizationUnitId { get; set; }

        /// <summary>
        ///     Gets or sets the responsible organization unit.
        /// </summary>
        /// <value>
        ///     The responsible organization unit.
        /// </value>
        public virtual OrganizationUnit ResponsibleOrganizationUnit { get; set; }

        /// <summary>
        ///     Id of the organization this contract was created under.
        /// </summary>
        /// <value>
        ///     The organization identifier.
        /// </value>
        public int OrganizationId { get; set; }

        /// <summary>
        ///     Gets or sets the organization this contract was created under.
        /// </summary>
        /// <value>
        ///     The organization.
        /// </value>
        public virtual Organization.Organization Organization { get; set; }

        /// <summary>
        ///     Id of the organization marked as supplier for this contract.
        /// </summary>
        /// <value>
        ///     The organization identifier.
        /// </value>
        public int? SupplierId { get; set; }

        /// <summary>
        ///     Gets or sets the organization marked as supplier for this contract.
        /// </summary>
        /// <value>
        ///     The organization.
        /// </value>
        public virtual Organization.Organization Supplier { get; set; }

        /// <summary>
        ///     Gets or sets the chosen procurement strategy option identifier. (Genanskaffelsesstrategi)
        /// </summary>
        /// <value>
        ///     Chosen procurement strategy identifier.
        /// </value>
        public int? ProcurementStrategyId { get; set; }

        /// <summary>
        ///     Gets or sets the chosen procurement strategy option. (Genanskaffelsesstrategi)
        /// </summary>
        /// <value>
        ///     Chosen procurement strategy.
        /// </value>
        public virtual ProcurementStrategyType ProcurementStrategy { get; set; }

        /// <summary>
        ///     Gets or sets the procurement plan half. (genanskaffelsesplan)
        /// </summary>
        /// <remarks>
        ///     The other part of this option is <see cref="ProcurementPlanYear" />
        /// </remarks>
        /// <value>
        ///     Can have a value between 1 and 4 for each of the quarters.
        /// </value>
        public int? ProcurementPlanQuarter { get; set; }

        /// <summary>
        ///     Gets or sets the procurement plan year. (genanskaffelsesplan)
        /// </summary>
        /// <remarks>
        ///     the other part of this option is <see cref="ProcurementPlanQuarter" />
        /// </remarks>
        /// <value>
        ///     The procurement plan year.
        /// </value>
        public int? ProcurementPlanYear { get; set; }

        /// <summary>
        ///     Gets or sets if procurement has been initiated (Genanskaffelse igangsat)
        /// </summary>
        /// <value>
        ///     Yes/No/Undecided procurment initiated.
        /// </value>
        public YesNoUndecidedOption? ProcurementInitiated { get; set; }

        /// <summary>
        ///     Gets or sets the chosen contract template identifier.
        /// </summary>
        /// <value>
        ///     Chosen contract template identifier.
        /// </value>
        public int? ContractTemplateId { get; set; }

        /// <summary>
        ///     Gets or sets the chosen contract template option.
        /// </summary>
        /// <value>
        ///     Chosen contract template.
        /// </value>
        public virtual ItContractTemplateType ContractTemplate { get; set; }

        /// <summary>
        ///     Gets or sets the chosen contract type option identifier.
        /// </summary>
        /// <value>
        ///     Chosen contract type identifier.
        /// </value>
        public int? ContractTypeId { get; set; }

        /// <summary>
        ///     Gets or sets the chosen type of the contract.
        /// </summary>
        /// <value>
        ///     The type of the contract.
        /// </value>
        public virtual ItContractType ContractType { get; set; }

        /// <summary>
        ///     Gets or sets the chosen purchase form option identifier.
        /// </summary>
        /// <value>
        ///     Chosen purchase form identifier.
        /// </value>
        public int? PurchaseFormId { get; set; }

        /// <summary>
        ///     Gets or sets the chosen purchase form option.
        /// </summary>
        /// <value>
        ///     Chosen purchase form.
        /// </value>
        public virtual PurchaseFormType PurchaseForm { get; set; }

        /// <summary>
        ///     Id of parent ItContract
        /// </summary>
        /// <value>
        ///     The parent identifier.
        /// </value>
        public int? ParentId { get; set; }

        /// <summary>
        ///     The parent ItContract
        /// </summary>
        /// <value>
        ///     The parent.
        /// </value>
        public virtual ItContract Parent { get; set; }

        public bool RequireValidParent { get; set; }

        /// <summary>
        ///     Gets or sets the contract children.
        /// </summary>
        /// <value>
        ///     The contract children.
        /// </value>
        public virtual ICollection<ItContract> Children { get; set; }


        /// <summary>
        ///     Id of criticality type ItContract
        /// </summary>
        /// <value>
        ///     The criticality type identifier.
        /// </value>
        public int? CriticalityId { get; set; }

        /// <summary>
        ///     The criticality of ItContract
        /// </summary>
        /// <value>
        ///     The criticality.
        /// </value>
        public virtual CriticalityType Criticality { get; set; }
        /// <summary>
        /// Read models
        /// </summary>
        public virtual ICollection<ItContractOverviewReadModel> OverviewReadModels { get; set; }

        #endregion

        #region Deadlines (aftalefrister)

        /// <summary>
        ///     When the contract began. (indgået)
        /// </summary>
        /// <value>
        ///     The concluded date.
        /// </value>
        public DateTime? Concluded { get; set; }

        /// <summary>
        ///     Gets or sets the duration in years. (varighed)
        /// </summary>
        /// <value>
        ///     The duration in years.
        /// </value>
        public int? DurationYears { get; set; }

        /// <summary>
        ///     Gets or sets the duration in months. (varighed)
        /// </summary>
        /// <value>
        ///     The duration in months.
        /// </value>
        public int? DurationMonths { get; set; }

        /// <summary>
        ///     Gets or sets the ongoing status. (løbende)
        /// </summary>
        /// <value>
        ///     Is the duration ongoing.
        /// </value>
        public bool DurationOngoing { get; set; }

        /// <summary>
        ///     Gets or sets the irrevocable to. (uopsigelig til)
        /// </summary>
        /// <value>
        ///     Irrevocable to date.
        /// </value>
        public DateTime? IrrevocableTo { get; set; }

        /// <summary>
        ///     When the contract expires. (udløbet)
        /// </summary>
        /// <value>
        ///     The expiration date.
        /// </value>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        ///     Date the contract ends. (opsagt)
        /// </summary>
        /// <value>
        ///     The termination date.
        /// </value>
        public DateTime? Terminated { get; set; }

        public int? TerminationDeadlineId { get; set; }

        /// <summary>
        ///     Gets or sets the termination deadline option. (opsigelsesfrist)
        /// </summary>
        /// <remarks>
        ///     Added months to the <see cref="Terminated" /> contract termination date before the contract expires.
        ///     It's a string but should be treated as an int.
        /// </remarks>
        /// <value>
        ///     The termination deadline.
        /// </value>
        public virtual TerminationDeadlineType TerminationDeadline { get; set; }

        public int? OptionExtendId { get; set; }
        public virtual OptionExtendType OptionExtend { get; set; }
        public int ExtendMultiplier { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <value>
        ///     (løbende)
        /// </value>
        public YearSegmentOption? Running { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <value>
        ///     (indtil udgangen af)
        /// </value>
        public YearSegmentOption? ByEnding { get; set; }

        #endregion

        #region Payment Model

        /// <summary>
        ///     Gets or sets the operation remuneration begun.
        /// </summary>
        /// <value>
        ///     The operation remuneration begun.
        /// </value>
        public DateTime? OperationRemunerationBegun { get; set; }

        public int? PaymentFreqencyId { get; set; }
        public virtual PaymentFreqencyType PaymentFreqency { get; set; }
        public int? PaymentModelId { get; set; }
        public virtual PaymentModelType PaymentModel { get; set; }
        public int? PriceRegulationId { get; set; }
        public virtual PriceRegulationType PriceRegulation { get; set; }

        #endregion

        #region Elementtypes

        public virtual ICollection<ItContractAgreementElementTypes> AssociatedAgreementElementTypes { get; set; }
        #endregion

        #region IT Systems

        /// <summary>
        ///     The (local usages of) it systems, that this contract is associated to.
        /// </summary>
        /// <value>
        ///     The associated system usages.
        /// </value>
        public virtual ICollection<ItContractItSystemUsage> AssociatedSystemUsages { get; set; }

        #endregion

        #region Economy Stream

        /// <summary>
        ///     Gets or sets the intern economy streams.
        /// </summary>
        /// <value>
        ///     The intern economy streams.
        /// </value>
        public virtual ICollection<EconomyStream> InternEconomyStreams { get; set; }

        /// <summary>
        ///     Gets or sets the extern economy streams.
        /// </summary>
        /// <value>
        ///     The extern economy streams.
        /// </value>
        public virtual ICollection<EconomyStream> ExternEconomyStreams { get; set; }

        public virtual ICollection<ExternalReference> ExternalReferences { get; set; }
        public ReferenceRootType GetRootType()
        {
            return ReferenceRootType.Contract;
        }

        public Result<ExternalReference, OperationError> AddExternalReference(ExternalReference newReference)
        {
            return new AddReferenceCommand(this).AddExternalReference(newReference);
        }

        public void ClearMasterReference()
        {
            Reference?.Track();
            Reference = null;
        }
        public Result<ExternalReference, OperationError> SetMasterReference(ExternalReference newReference)
        {
            Reference = newReference;
            return newReference;
        }

        #endregion

        public virtual ICollection<DataProcessingRegistration> DataProcessingRegistrationsWhereContractIsMainContract { get; set; }

        public virtual ICollection<SystemRelation> AssociatedSystemRelations { get; set; }

        public virtual ICollection<GDPR.DataProcessingRegistration> DataProcessingRegistrations { get; set; }

        public Result<DataProcessingRegistration, OperationError> AssignDataProcessingRegistration(DataProcessingRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            if (registration.OrganizationId != OrganizationId)
                return new OperationError("Cannot assign Data Processing Registration to Contract within different Organization", OperationFailure.BadInput);

            if (GetAssignedDataProcessingRegistration(registration.Id).HasValue)
                return new OperationError("Data processing registration is already assigned", OperationFailure.Conflict);

            DataProcessingRegistrations.Add(registration);

            return registration;
        }

        public Result<DataProcessingRegistration, OperationError> RemoveDataProcessingRegistration(DataProcessingRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            if (GetAssignedDataProcessingRegistration(registration.Id).IsNone)
                return new OperationError("Data processing registration not assigned", OperationFailure.BadInput);

            DataProcessingRegistrations.Remove(registration);

            return registration;
        }

        private Maybe<DataProcessingRegistration> GetAssignedDataProcessingRegistration(int dataProcessingRegistrationId)
        {
            return DataProcessingRegistrations.FirstOrDefault(x => x.Id == dataProcessingRegistrationId).FromNullable();
        }

        public override ItContractRight CreateNewRight(ItContractRole role, User user)
        {
            return new ItContractRight()
            {
                Role = role,
                User = user,
                Object = this
            };
        }

        public void ResetContractType()
        {
            ContractType?.Track();
            ContractType = null;
        }

        public void ResetContractTemplate()
        {
            ContractTemplate?.Track();
            ContractTemplate = null;
        }

        public void ResetCriticality()
        {
            Criticality?.Track();
            Criticality = null;
        }

        public Maybe<OperationError> UpdateContractValidityPeriod(DateTime? newValidFrom, DateTime? newValidTo)
        {
            var validFromDate = newValidFrom?.Date;
            var validToDate = newValidTo?.Date;

            if (validFromDate.HasValue && validToDate.HasValue && validFromDate.Value.Date > validToDate.Value.Date)
            {
                return new OperationError("ValidTo must equal or proceed ValidFrom", OperationFailure.BadInput);
            }

            Concluded = validFromDate;

            ExpirationDate = validToDate;

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> SetAgreementElements(IEnumerable<AgreementElementType> agreementElementTypes)
        {
            var agreementElements = agreementElementTypes.Select(type => new ItContractAgreementElementTypes()
            {
                ItContract = this,
                AgreementElementType = type
            }).ToList();

            if (agreementElements.Select(x => x.AgreementElementType.Uuid).Distinct().Count() != agreementElements.Count)
                return new OperationError("agreement elements must not contain duplicates", OperationFailure.BadInput);

            agreementElements.MirrorTo(AssociatedAgreementElementTypes, assignment => assignment.AgreementElementType.Uuid);

            return Maybe<OperationError>.None;
        }

        public void ClearParent()
        {
            Parent?.Track();
            Parent = null;
        }

        public Maybe<OperationError> SetParent(ItContract newParent)
        {
            if (OrganizationId == newParent.OrganizationId)
            {
                Parent = newParent;
                return Maybe<OperationError>.None;
            }
            return new OperationError("Parent and child contracts must be in same organization", OperationFailure.BadInput);
        }

        public Maybe<OperationError> SetResponsibleOrganizationUnit(Guid organizationUnitUuid)
        {
            if (organizationUnitUuid != ResponsibleOrganizationUnit?.Uuid)
            {
                var organizationUnit = Organization.GetOrganizationUnit(organizationUnitUuid);
                if (organizationUnit.IsNone)
                {
                    return new OperationError("UUID of responsible organization unit does not match an organization unit on this contract's organization", OperationFailure.BadInput);
                }

                ResponsibleOrganizationUnit = organizationUnit.Value;
            }

            return Maybe<OperationError>.None;
        }

        public void ResetResponsibleOrganizationUnit()
        {
            ResponsibleOrganizationUnit?.Track();
            ResponsibleOrganizationUnit = null;
        }

        public void ResetProcurementStrategy()
        {
            ProcurementStrategy?.Track();
            ProcurementStrategy = null;
        }

        public void ResetPurchaseForm()
        {
            PurchaseForm?.Track();
            PurchaseForm = null;
        }

        public void ResetProcurementPlan()
        {
            ProcurementPlanQuarter = null;
            ProcurementPlanYear = null;
        }

        public Maybe<OperationError> UpdateProcurementPlan((byte quarter, int year) plan)
        {
            var (quarter, year) = plan;
            if (quarter is < 1 or > 4)
            {
                return new OperationError("Quarter Of Year has to be either 1, 2, 3 or 4", OperationFailure.BadInput);
            }

            ProcurementPlanQuarter = quarter;
            ProcurementPlanYear = year;
            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> AssignSystemUsage(ItSystemUsage.ItSystemUsage systemUsage)
        {
            if (systemUsage == null) throw new ArgumentNullException(nameof(systemUsage));

            if (systemUsage.OrganizationId != OrganizationId)
                return new OperationError("Cannot assign It System Usage to Contract within different Organization", OperationFailure.BadInput);

            if (AssociatedSystemUsages.Any(x => x.ItSystemUsageId == systemUsage.Id))
                return new OperationError($"It System Usage with Id: {systemUsage.Id}, already assigned to Contract", OperationFailure.Conflict);

            var newAssign = new ItContractItSystemUsage
            {
                ItContract = this,
                ItSystemUsage = systemUsage
            };

            AssociatedSystemUsages.Add(newAssign);

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> RemoveSystemUsage(ItSystemUsage.ItSystemUsage systemUsage)
        {
            if (systemUsage == null) throw new ArgumentNullException(nameof(systemUsage));

            var toBeRemoved = AssociatedSystemUsages.Where(x => x.ItSystemUsageId == systemUsage.Id).ToList();

            foreach (var contractUsageToRemove in toBeRemoved)
            {
                var removeSucceeded = AssociatedSystemUsages.Remove(contractUsageToRemove);
                if (!removeSucceeded)
                    return new OperationError($"Failed to remove AssociatedSystemUsage with Id: {systemUsage.Id}", OperationFailure.BadState);
            }

            return Maybe<OperationError>.None;
        }

        public void ResetSupplierOrganization()
        {
            Supplier.Track();
            Supplier = null;
        }

        public Maybe<OperationError> SetSupplierOrganization(Organization.Organization organization)
        {
            if (organization == null)
                throw new ArgumentNullException(nameof(organization));

            if (Supplier == null || organization.Uuid != Supplier.Uuid)
                Supplier = organization;

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> UpdateExtendMultiplier(int extendMultiplier)
        {
            if (extendMultiplier < 0)
                return new OperationError($"{nameof(extendMultiplier)} must be above or equal to 0", OperationFailure.BadInput);

            ExtendMultiplier = extendMultiplier;
            return Maybe<OperationError>.None;
        }

        public void ResetExtensionOption()
        {
            OptionExtend.Track();
            OptionExtend = null;
        }

        public Maybe<OperationError> UpdateDuration(int? durationMonths, int? durationYears, bool ongoing)
        {
            if (ongoing && (durationMonths.HasValue || durationYears.HasValue))
                return new OperationError($"If duration is ongoing then {nameof(durationMonths)} and {nameof(durationYears)} must be null", OperationFailure.BadInput);

            if (durationYears.GetValueOrDefault() < 0)
                return new OperationError($"{nameof(durationYears)} cannot be below 0", OperationFailure.BadInput);

            var months = durationMonths.GetValueOrDefault();

            if (months is < 0 or > 11)
                return new OperationError($"{nameof(durationMonths)} cannot be below 0 or above 11", OperationFailure.BadInput);

            DurationOngoing = ongoing;
            DurationYears = durationYears;
            DurationMonths = durationMonths;

            return Maybe<OperationError>.None;
        }

        public void ResetPaymentFrequency()
        {
            PaymentFreqency.Track();
            PaymentFreqency = null;
        }

        public void ResetPaymentModel()
        {
            PaymentModel.Track();
            PaymentModel = null;
        }

        public void ResetPriceRegulation()
        {
            PriceRegulation.Track();
            PriceRegulation = null;
        }

        public void ResetInternalEconomyStreams()
        {
            InternEconomyStreams.Clear();
        }

        public Maybe<OperationError> AddInternalEconomyStream(Guid? optionalOrganizationUnitUuid, int acquisition, int operation, int other, string accountingEntry, TrafficLight auditStatus, DateTime? auditDate, string note)
        {
            return AddEconomyStream(optionalOrganizationUnitUuid, acquisition, operation, other, accountingEntry, auditStatus, auditDate, note, true);
        }

        public Maybe<OperationError> AddExternalEconomyStream(Guid? optionalOrganizationUnitUuid, int acquisition, int operation, int other, string accountingEntry, TrafficLight auditStatus, DateTime? auditDate, string note)
        {
            return AddEconomyStream(optionalOrganizationUnitUuid, acquisition, operation, other, accountingEntry, auditStatus, auditDate, note, false);
        }

        public IEnumerable<EconomyStream> GetAllPayments()
        {
            return ExternEconomyStreams.ToList().Concat(InternEconomyStreams.ToList());
        }

        public IEnumerable<EconomyStream> GetInternalPaymentsForUnit(int unitId)
        {
            return InternEconomyStreams.Where(x => x.OrganizationUnitId == unitId).ToList();
        }

        public IEnumerable<EconomyStream> GetExternalPaymentsForUnit(int unitId)
        {
            return ExternEconomyStreams.Where(x => x.OrganizationUnitId == unitId).ToList();
        }

        public Maybe<OperationError> ResetEconomyStreamOrganizationUnit(int id, bool isInternal)
        {
            return isInternal
                ? ResetEconomyStreamOrganizationUnit(id, InternEconomyStreams)
                : ResetEconomyStreamOrganizationUnit(id, ExternEconomyStreams);
        }

        private static Maybe<OperationError> ResetEconomyStreamOrganizationUnit(int id, IEnumerable<EconomyStream> economyStreams)
        {
            var stream = economyStreams.FirstOrDefault(x => x.Id == id);
            if (stream == null)
                return new OperationError($"EconomyStream with id: {id} was not found", OperationFailure.NotFound);

            stream.ResetOrganizationUnit();

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> TransferEconomyStream(int id, Guid targetUnitUuid, bool isInternal)
        {
            return Organization.GetOrganizationUnit(targetUnitUuid)
                .Match
                (
                    targetUnit => isInternal
                            ? TransferEconomyStream(id, targetUnit, InternEconomyStreams)
                            : TransferEconomyStream(id, targetUnit, ExternEconomyStreams),
                    () => new OperationError($"Organization unit with uuid: {targetUnitUuid} was not found", OperationFailure.NotFound)
                );
        }

        private static Maybe<OperationError> TransferEconomyStream(int id, OrganizationUnit targetUnit, IEnumerable<EconomyStream> economyStreams)
        {
            var stream = economyStreams.FirstOrDefault(x => x.Id == id);
            if (stream == null)
                return new OperationError($"EconomyStream with id: {id} was not found", OperationFailure.NotFound);

            stream.SetOrganizationUnit(targetUnit);
            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> AddEconomyStream(
            Guid? optionalOrganizationUnitUuid,
            int acquisition,
            int operation,
            int other,
            string accountingEntry,
            TrafficLight auditStatus,
            DateTime? auditDate,
            string note,
            bool internalStream)
        {
            var organizationUnit = Maybe<OrganizationUnit>.None;
            if (optionalOrganizationUnitUuid.HasValue)
            {
                organizationUnit = Organization.GetOrganizationUnit(optionalOrganizationUnitUuid.Value);
                if (organizationUnit.IsNone)
                    return new OperationError($"Organization unit with uuid:{optionalOrganizationUnitUuid.Value} is not part of the contract's organization", OperationFailure.BadInput);
            }

            var economyStream = internalStream ?
                EconomyStream.CreateInternalEconomyStream(this, organizationUnit.GetValueOrDefault(), acquisition, operation, other, accountingEntry, auditStatus, auditDate, note) :
                EconomyStream.CreateExternalEconomyStream(this, organizationUnit.GetValueOrDefault(), acquisition, operation, other, accountingEntry, auditStatus, auditDate, note);

            (internalStream ? InternEconomyStreams : ExternEconomyStreams).Add(economyStream);

            return Maybe<OperationError>.None;
        }

        public void ResetExternalEconomyStreams()
        {
            ExternEconomyStreams.Clear();
        }

        public void ResetNoticePeriod()
        {
            TerminationDeadline.Track();
            TerminationDeadline = null;
        }

        public void MarkAsDirty()
        {
            LastChanged = DateTime.UtcNow;
        }

        public void SetRequireValidParent(bool requireValidParent)
        {
            RequireValidParent = requireValidParent;
        }
    }
}