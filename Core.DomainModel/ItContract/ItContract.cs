using System;
using System.Collections.Generic;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainModel.Result;
using Core.DomainModel.GDPR;
using System.Linq;
using Infrastructure.Services.Types;
using Core.DomainModel.Notification;

// ReSharper disable VirtualMemberCallInConstructor

namespace Core.DomainModel.ItContract
{
    /// <summary>
    ///     Contains info about an it contract
    /// </summary>
    public class ItContract : HasRightsEntity<ItContract, ItContractRight, ItContractRole>, IHasReferences, IHierarchy<ItContract>, IContractModule, IOwnedByOrganization, IHasName, IEntityWithExternalReferences, IEntityWithAdvices, IEntityWithUserNotification, IHasUuid
    {

        public ItContract()
        {
            Children = new List<ItContract>();
            AssociatedAgreementElementTypes = new List<ItContractAgreementElementTypes>();
            AssociatedSystemUsages = new List<ItContractItSystemUsage>();
            PaymentMilestones = new List<PaymentMilestone>();
            InternEconomyStreams = new List<EconomyStream>();
            ExternEconomyStreams = new List<EconomyStream>();
            ExternalReferences = new List<ExternalReference>();
            DataProcessingRegistrations = new List<DataProcessingRegistration>();
            UserNotifications = new List<UserNotification>();
            Uuid = Guid.NewGuid();
        }

        public Guid Uuid { get; set; }

        /// <summary>
        ///     Whether the contract is active or not
        /// </summary>
        public bool IsActive
        {
            get
            {
                if (!Active)
                {
                    var today = DateTime.UtcNow;
                    var startDate = Concluded ?? today;
                    var endDate = DateTime.MaxValue;

                    if (ExpirationDate.HasValue && ExpirationDate.Value.Date != DateTime.MaxValue.Date)
                    {
                        endDate = new DateTime(ExpirationDate.Value.Year, ExpirationDate.Value.Month, ExpirationDate.Value.Day, 23, 59, 59);
                    }

                    if (Terminated.HasValue)
                    {
                        var terminationDate = Terminated;
                        if (TerminationDeadline != null)
                        {
                            int deadline;
                            int.TryParse(TerminationDeadline.Name, out deadline);
                            terminationDate = Terminated.Value.AddMonths(deadline);
                        }
                        // indgået-dato <= dags dato <= opsagt-dato + opsigelsesfrist
                        return today >= startDate.Date && today <= terminationDate.Value.Date.AddDays(1).AddTicks(-1);
                    }

                    // indgået-dato <= dags dato <= udløbs-dato
                    return today >= startDate.Date && today <= endDate;
                }
                return Active;
            }
        }

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
        ///     Gets or sets a reference to relevant documents in an extern ESDH system.
        /// </summary>
        /// <value>
        ///     Extern reference  to ESDH system.
        /// </value>
        public string Esdh { get; set; }

        /// <summary>
        ///     Gets or sets a path to relevant documents in a folder.
        /// </summary>
        /// <value>
        ///     Path to folder containing relevant documents.
        /// </value>
        public string Folder { get; set; }

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
        ///     Gets or sets the chosen procurement strategy option identifier. (udbudsstrategi)
        /// </summary>
        /// <value>
        ///     Chosen procurement strategy identifier.
        /// </value>
        public int? ProcurementStrategyId { get; set; }

        /// <summary>
        ///     Gets or sets the chosen procurement strategy option. (udbudsstrategi)
        /// </summary>
        /// <value>
        ///     Chosen procurement strategy.
        /// </value>
        public virtual ProcurementStrategyType ProcurementStrategy { get; set; }

        /// <summary>
        ///     Gets or sets the procurement plan half. (udbudsplan)
        /// </summary>
        /// <remarks>
        ///     The other part of this option is <see cref="ProcurementPlanYear" />
        /// </remarks>
        /// <value>
        ///     Can be either 1 for the 1st and 2nd quarter or 2 for the 3rd and 4th quarter.
        /// </value>
        public int? ProcurementPlanHalf { get; set; }

        /// <summary>
        ///     Gets or sets the procurement plan year. (udbudsplan)
        /// </summary>
        /// <remarks>
        ///     the other part of this option is <see cref="ProcurementPlanHalf" />
        /// </remarks>
        /// <value>
        ///     The procurement plan year.
        /// </value>
        public int? ProcurementPlanYear { get; set; }

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

        /// <summary>
        ///     Gets or sets the contract children.
        /// </summary>
        /// <value>
        ///     The contract children.
        /// </value>
        public virtual ICollection<ItContract> Children { get; set; }

        #endregion

        #region Deadlines (aftalefrister)

        /// <summary>
        ///     Gets or sets the operation test expected.
        /// </summary>
        /// <remarks>
        ///     Is called "aftalefrister -> funktionsprøve: forventet"
        /// </remarks>
        /// <value>
        ///     The operation test expected.
        /// </value>
        public DateTime? OperationTestExpected { get; set; }

        /// <summary>
        ///     Gets or sets the operation test approved.
        /// </summary>
        /// <remarks>
        ///     Is called "aftalefrister -> funktionsprøve: godkendt"
        /// </remarks>
        /// <value>
        ///     The operation test approved.
        /// </value>
        public DateTime? OperationTestApproved { get; set; }

        /// <summary>
        ///     Gets or sets the operational acceptance test expected.
        /// </summary>
        /// <remarks>
        ///     Is called "aftalefrister -> driftovertagelsesprøve: forventet"
        /// </remarks>
        /// <value>
        ///     The operational acceptance test expected.
        /// </value>
        public DateTime? OperationalAcceptanceTestExpected { get; set; }

        /// <summary>
        ///     Gets or sets the operational acceptance test approved.
        /// </summary>
        /// <remarks>
        ///     Is called "aftalefrister -> driftovertagelsesprøve: godkendt"
        /// </remarks>
        /// <value>
        ///     The operational acceptance test approved.
        /// </value>
        public DateTime? OperationalAcceptanceTestApproved { get; set; }

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

        /// <summary>
        ///     Gets or sets the payment milestones.
        /// </summary>
        /// <value>
        ///     The payment milestones.
        /// </value>
        public virtual ICollection<PaymentMilestone> PaymentMilestones { get; set; }

        public int? OptionExtendId { get; set; }
        public virtual OptionExtendType OptionExtend { get; set; }
        public int ExtendMultiplier { get; set; }

        /// <summary>
        ///     Gets or sets the handover trials.
        /// </summary>
        /// <value>
        ///     The handover trials.
        /// </value>
        public virtual ICollection<HandoverTrial> HandoverTrials { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <value>
        ///     (løbende)
        /// </value>
        public string Running { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <value>
        ///     (indtil udgangen af)
        /// </value>
        public string ByEnding { get; set; }

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

        public Result<ExternalReference, OperationError> SetMasterReference(ExternalReference newReference)
        {
            Reference = newReference;
            return newReference;
        }

        #endregion

        public virtual ICollection<SystemRelation> AssociatedSystemRelations { get; set; }

        public virtual ICollection<GDPR.DataProcessingRegistration> DataProcessingRegistrations { get; set; }

        public Result<DataProcessingRegistration, OperationError> AssignDataProcessingRegistration(DataProcessingRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

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
    }
}