using System;
using System.Collections.Generic;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainModel.ItContract
{
    /// <summary>
    /// Contains info about an it contract
    /// </summary>
    public class ItContract : HasRightsEntity<ItContract, ItContractRight, ItContractRole>, IHierarchy<ItContract>, IContextAware, IContractModule
    {
        public ItContract()
        {
            this.AgreementElements = new List<AgreementElementType>();
            this.Children = new List<ItContract>();
            this.AssociatedSystemUsages = new List<ItContractItSystemUsage>();
            this.AssociatedInterfaceUsages = new List<ItInterfaceUsage>();
            this.AssociatedInterfaceExposures = new List<ItInterfaceExhibitUsage>();
            this.PaymentMilestones = new List<PaymentMilestone>();
            this.InternEconomyStreams = new List<EconomyStream>();
            this.ExternEconomyStreams = new List<EconomyStream>();
            this.Advices = new List<Advice>();
        }

        #region Master

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        public string Note { get; set; }
        /// <summary>
        /// Gets or sets it contract identifier defined by the user.
        /// </summary>
        /// <remarks>
        /// This identifier is NOT the primary key.
        /// </remarks>
        /// <value>
        /// User defined it contract identifier.
        /// </value>
        public string ItContractId { get; set; }
        /// <summary>
        /// Gets or sets a reference to relevant documents in an extern ESDH system.
        /// </summary>
        /// <value>
        /// Extern reference  to ESDH system.
        /// </value>
        public string Esdh { get; set; }
        /// <summary>
        /// Gets or sets a path to relevant documents in a folder.
        /// </summary>
        /// <value>
        /// Path to folder containing relevant documents.
        /// </value>
        public string Folder { get; set; }
        /// <summary>
        /// Gets or sets the supplier contract signer.
        /// </summary>
        /// <value>
        /// The supplier contract signer.
        /// </value>
        public string SupplierContractSigner { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance has supplier signed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has supplier signed; otherwise, <c>false</c>.
        /// </value>
        public bool HasSupplierSigned { get; set; }
        /// <summary>
        /// Gets or sets the supplier signed date.
        /// </summary>
        /// <value>
        /// The supplier signed date.
        /// </value>
        public DateTime? SupplierSignedDate { get; set; }
        /// <summary>
        /// Gets or sets the contract signer identifier.
        /// </summary>
        /// <value>
        /// The contract signer identifier.
        /// </value>
        public int? ContractSignerId { get; set; }
        /// <summary>
        /// Gets or sets the contract signer.
        /// </summary>
        /// <value>
        /// The contract signer.
        /// </value>
        public virtual User ContractSigner { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this contract is signed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if contract is signed; otherwise, <c>false</c>.
        /// </value>
        public bool IsSigned { get; set; }
        /// <summary>
        /// Gets or sets the signed date.
        /// </summary>
        /// <value>
        /// The signed date.
        /// </value>
        public DateTime? SignedDate { get; set; }

        /// <summary>
        /// The chosen responsible org unit for this contract
        /// </summary>
        /// <value>
        /// The responsible organization unit identifier.
        /// </value>
        public int? ResponsibleOrganizationUnitId { get; set; }
        /// <summary>
        /// Gets or sets the responsible organization unit.
        /// </summary>
        /// <value>
        /// The responsible organization unit.
        /// </value>
        public virtual OrganizationUnit ResponsibleOrganizationUnit { get; set; }

        /// <summary>
        /// Id of the organization this contract was created under.
        /// </summary>
        /// <value>
        /// The organization identifier.
        /// </value>
        public int OrganizationId { get; set; }
        /// <summary>
        /// Gets or sets the organization this contract was created under.
        /// </summary>
        /// <value>
        /// The organization.
        /// </value>
        public virtual Organization Organization { get; set; }

        /// <summary>
        /// Id of the organization marked as supplier for this contract.
        /// </summary>
        /// <value>
        /// The organization identifier.
        /// </value>
        public int? SupplierId { get; set; }
        /// <summary>
        /// Gets or sets the organization marked as supplier for this contract.
        /// </summary>
        /// <value>
        /// The organization.
        /// </value>
        public virtual Organization Supplier { get; set; }
        /// <summary>
        /// Gets or sets the chosen procurement strategy option identifier. (udbudsstrategi)
        /// </summary>
        /// <value>
        /// Chosen procurement strategy identifier.
        /// </value>
        public int? ProcurementStrategyId { get; set; }
        /// <summary>
        /// Gets or sets the chosen procurement strategy option. (udbudsstrategi)
        /// </summary>
        /// <value>
        /// Chosen procurement strategy.
        /// </value>
        public virtual ProcurementStrategyType ProcurementStrategy { get; set; }
        /// <summary>
        /// Gets or sets the procurement plan half. (udbudsplan)
        /// </summary>
        /// <remarks>
        /// The other part of this option is <see cref="ProcurementPlanYear"/>
        /// </remarks>
        /// <value>
        /// Can be either 1 for the 1st and 2nd quarter or 2 for the 3rd and 4th quarter.
        /// </value>
        public int? ProcurementPlanHalf { get; set; }
        /// <summary>
        /// Gets or sets the procurement plan year. (udbudsplan)
        /// </summary>
        /// <remarks>
        /// the other part of this option is <see cref="ProcurementPlanHalf"/>
        /// </remarks>
        /// <value>
        /// The procurement plan year.
        /// </value>
        public int? ProcurementPlanYear { get; set; }
        /// <summary>
        /// Gets or sets the chosen contract template identifier.
        /// </summary>
        /// <value>
        /// Chosen contract template identifier.
        /// </value>
        public int? ContractTemplateId { get; set; }
        /// <summary>
        /// Gets or sets the chosen contract template option.
        /// </summary>
        /// <value>
        /// Chosen contract template.
        /// </value>
        public virtual ItContractTemplateType ContractTemplate { get; set; }
        /// <summary>
        /// Gets or sets the chosen contract type option identifier.
        /// </summary>
        /// <value>
        /// Chosen contract type identifier.
        /// </value>
        public int? ContractTypeId { get; set; }
        /// <summary>
        /// Gets or sets the chosen type of the contract.
        /// </summary>
        /// <value>
        /// The type of the contract.
        /// </value>
        public virtual ItContractType ContractType { get; set; }
        /// <summary>
        /// Gets or sets the chosen purchase form option identifier.
        /// </summary>
        /// <value>
        /// Chosen purchase form identifier.
        /// </value>
        public int? PurchaseFormId { get; set; }
        /// <summary>
        /// Gets or sets the chosen purchase form option.
        /// </summary>
        /// <value>
        /// Chosen purchase form.
        /// </value>
        public virtual PurchaseFormType PurchaseForm { get; set; }
        /// <summary>
        /// Id of parent ItContract
        /// </summary>
        /// <value>
        /// The parent identifier.
        /// </value>
        public int? ParentId { get; set; }
        /// <summary>
        /// The parent ItContract
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public virtual ItContract Parent { get; set; }
        /// <summary>
        /// Gets or sets the contract children.
        /// </summary>
        /// <value>
        /// The contract children.
        /// </value>
        public virtual ICollection<ItContract> Children { get; set; }
        /// <summary>
        /// Gets or sets the chosen agreement elements.
        /// </summary>
        /// <value>
        /// Chosen agreement elements.
        /// </value>
        public virtual ICollection<AgreementElementType> AgreementElements { get; set; }

        #endregion

        #region Deadlines (aftalefrister)
        /// <summary>
        /// Gets or sets the operation test expected.
        /// </summary>
        /// <remarks>
        /// Is called "aftalefrister -> funktionspr�ve: forventet"
        /// </remarks>
        /// <value>
        /// The operation test expected.
        /// </value>
        public DateTime? OperationTestExpected { get; set; }
        /// <summary>
        /// Gets or sets the operation test approved.
        /// </summary>
        /// <remarks>
        /// Is called "aftalefrister -> funktionspr�ve: godkendt"
        /// </remarks>
        /// <value>
        /// The operation test approved.
        /// </value>
        public DateTime? OperationTestApproved { get; set; }
        /// <summary>
        /// Gets or sets the operational acceptance test expected.
        /// </summary>
        /// <remarks>
        /// Is called "aftalefrister -> driftovertagelsespr�ve: forventet"
        /// </remarks>
        /// <value>
        /// The operational acceptance test expected.
        /// </value>
        public DateTime? OperationalAcceptanceTestExpected { get; set; }
        /// <summary>
        /// Gets or sets the operational acceptance test approved.
        /// </summary>
        /// <remarks>
        /// Is called "aftalefrister -> driftovertagelsespr�ve: godkendt"
        /// </remarks>
        /// <value>
        /// The operational acceptance test approved.
        /// </value>
        public DateTime? OperationalAcceptanceTestApproved { get; set; }
        /// <summary>
        /// When the contract began. (indg�et)
        /// </summary>
        /// <value>
        /// The concluded date.
        /// </value>
        public DateTime? Concluded { get; set; }
        /// <summary>
        /// Gets or sets the duration. (varighed)
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        public int? Duration { get; set; }
        /// <summary>
        /// Gets or sets the irrevocable to. (uopsigelig til)
        /// </summary>
        /// <value>
        /// Irrevocable to date.
        /// </value>
        public DateTime? IrrevocableTo { get; set; }
        /// <summary>
        /// When the contract expires. (udl�bet)
        /// </summary>
        /// <value>
        /// The expiration date.
        /// </value>
        public DateTime? ExpirationDate { get; set; }
        /// <summary>
        /// Date the contract ends. (opsagt)
        /// </summary>
        /// <value>
        /// The termination date.
        /// </value>
        public DateTime? Terminated { get; set; }
        public int? TerminationDeadlineId { get; set; }
        /// <summary>
        /// Gets or sets the termination deadline option. (opsigelsesfrist)
        /// </summary>
        /// <remarks>
        /// Added months to the <see cref="Terminated"/> contract termination date before the contract expires.
        /// It's a string but should be treated as an int.
        /// TODO perhaps a redesign of OptionEntity is in order
        /// </remarks>
        /// <value>
        /// The termination deadline.
        /// </value>
        public virtual TerminationDeadlineType TerminationDeadline { get; set; }
        /// <summary>
        /// Gets or sets the payment milestones.
        /// </summary>
        /// <value>
        /// The payment milestones.
        /// </value>
        public virtual ICollection<PaymentMilestone> PaymentMilestones { get; set; }
        public int? OptionExtendId { get; set; }
        public virtual OptionExtendType OptionExtend { get; set; }
        public int ExtendMultiplier { get; set; }
        /// <summary>
        /// Gets or sets the handover trials.
        /// </summary>
        /// <value>
        /// The handover trials.
        /// </value>
        public virtual ICollection<HandoverTrial> HandoverTrials { get; set; }

        #endregion

        #region Payment Model

        /// <summary>
        /// Gets or sets the operation remuneration begun.
        /// </summary>
        /// <value>
        /// The operation remuneration begun.
        /// </value>
        public DateTime? OperationRemunerationBegun { get; set; }
        public int? PaymentFreqencyId { get; set; }
        public virtual PaymentFreqencyType PaymentFreqency { get; set; }
        public int? PaymentModelId { get; set; }
        public virtual PaymentModelType PaymentModel { get; set; }
        public int? PriceRegulationId { get; set; }
        public virtual PriceRegulationType PriceRegulation { get; set; }

        #endregion

        #region IT Systems

        /// <summary>
        /// The (local usages of) it systems, that this contract is associated to.
        /// </summary>
        /// <value>
        /// The associated system usages.
        /// </value>
        public virtual ICollection<ItContractItSystemUsage> AssociatedSystemUsages { get; set; }

        /// <summary>
        /// The interface usages that the contract is associated to.
        /// </summary>
        /// <value>
        /// The associated interface usages.
        /// </value>
        public virtual ICollection<ItInterfaceUsage> AssociatedInterfaceUsages { get; set; }
        /// <summary>
        /// Gets or sets the associated interface exposures.
        /// </summary>
        /// <value>
        /// The associated interface exposures.
        /// </value>
        public virtual ICollection<ItInterfaceExhibitUsage> AssociatedInterfaceExposures { get; set; }

        #endregion

        #region Economy Stream

        /// <summary>
        /// Gets or sets the intern economy streams.
        /// </summary>
        /// <value>
        /// The intern economy streams.
        /// </value>
        public virtual ICollection<EconomyStream> InternEconomyStreams { get; set; }
        /// <summary>
        /// Gets or sets the extern economy streams.
        /// </summary>
        /// <value>
        /// The extern economy streams.
        /// </value>
        public virtual ICollection<EconomyStream> ExternEconomyStreams { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets the advices.
        /// </summary>
        /// <value>
        /// The advices.
        /// </value>
        public virtual ICollection<Advice> Advices { get; set; }

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access; otherwise, <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            if (ContractSignerId == user.Id)
                return true;

            return base.HasUserWriteAccess(user);
        }

        /// <summary>
        /// Determines whether this instance is within a given organizational context.
        /// </summary>
        /// <param name="organizationId">The organization identifier (context) the user is accessing from.</param>
        /// <returns>
        ///   <c>true</c> if this instance is in the organizational context, otherwise <c>false</c>.
        /// </returns>
        public bool IsInContext(int organizationId)
        {
            return OrganizationId == organizationId;
        }

        /// <summary>
        /// Whether the contract is active or not
        /// </summary>
        public bool IsActive
        {
            get
            {
                var today = DateTime.UtcNow;
                var startDate = Concluded ?? today;
                var endDate = ExpirationDate ?? DateTime.MaxValue;

                if (Terminated.HasValue)
                {
                    var terminationDate = Terminated;
                    if (TerminationDeadline != null)
                    {
                        int deadline;
                        int.TryParse(TerminationDeadline.Name, out deadline);
                        terminationDate = Terminated.Value.AddMonths(deadline);
                    }
                    // indg�et-dato <= dags dato <= opsagt-dato + opsigelsesfrist
                    return today >= startDate && today <= terminationDate;
                }

                // indg�et-dato <= dags dato <= udl�bs-dato
                return today >= startDate && today <= endDate;
            }
        }
    }
}
