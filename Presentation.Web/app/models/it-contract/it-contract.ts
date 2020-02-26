module Kitos.Models.ItContract {
    /** Contains info about an it contract */
    export interface IItContract extends IEntity {
        /** Gets or sets the name. */
        Name: string;
        /** Gets or sets the note. */
        Note: string;
        /** Gets or sets it contract identifier defined by the user. */
        ItContractId: string;
        /** Gets or sets a reference to relevant documents in an extern ESDH system. */
        Esdh: string;
        /** Gets or sets a path to relevant documents in a folder. */
        Folder: string;
        /** Gets or sets the supplier contract signer. */
        SupplierContractSigner: string;
        /** Gets or sets a value indicating whether this instance has supplier signed. */
        HasSupplierSigned: boolean;
        /** Gets or sets the supplier signed date. */
        SupplierSignedDate: Date;
        /** Gets or sets the contract signer. */
        ContractSigner: string;
        /** Gets or sets a value indicating whether this contract is signed. */
        IsSigned: boolean;
        /** Gets or sets the signed date. */
        SignedDate: Date;
        /** The chosen responsible org unit for this contract */
        ResponsibleOrganizationUnitId: number;
        /** Gets or sets the responsible organization unit. */
        ResponsibleOrganizationUnit: IOrganizationUnit;
        /** Id of the organization this contract was created under. */
        OrganizationId: number;
        /** Gets or sets the organization this contract was created under. */
        Organization: IOrganization;
        /** Id of the organization marked as supplier for this contract. */
        SupplierId: number;
        /** Gets or sets the organization marked as supplier for this contract. */
        Supplier: IOrganization;
        /** Gets or sets the chosen procurement strategy option identifier. (udbudsstrategi) */
        ProcurementStrategyId: number;
        /** Gets or sets the chosen procurement strategy option. (udbudsstrategi) */
        ProcurementStrategy: IProcurementStrategy;
        /** Gets or sets the procurement plan half. (udbudsplan) */
        ProcurementPlanHalf: number;
        /** Gets or sets the procurement plan year. (udbudsplan) */
        ProcurementPlanYear: number;
        /** Gets or sets the chosen contract template identifier. */
        ContractTemplateId: number;
        /** Gets or sets the chosen contract template option. */
        ContractTemplate: IContractTemplate;
        /** Gets or sets the chosen contract type option identifier. */
        ContractTypeId: number;
        /** Gets or sets the chosen type of the contract. */
        ContractType: IContractType;
        /** Gets or sets the chosen purchase form option identifier. */
        PurchaseFormId: number;
        /** Gets or sets the chosen purchase form option. */
        PurchaseForm: IPurchaseForm;
        /** Id of parent ItContract */
        ParentId: number;
        /** The parent ItContract */
        Parent: IItContract;
        /** Gets or sets the contract children. */
        Children: Array<IItContract>;
        /** Gets or sets the chosen agreement elements. */
        AgreementElements: Array<IAgreementElement>;

        /** Gets or sets the operation test expected. */
        OperationTestExpected: Date;
        /** Gets or sets the operation test approved. */
        OperationTestApproved: Date;
        /** Gets or sets the operational acceptance test expected. */
        OperationalAcceptanceTestExpected: Date;
        /** Gets or sets the operational acceptance test approved. */
        OperationalAcceptanceTestApproved: Date;
        /** When the contract began. (indgået) */
        Concluded: Date;
        /** Gets or sets the duration in years. (varighed) */
        DurationYears: number;
        /** Gets or sets the duration in months. (varighed) */
        DurationMonths: number;
        /** Gets or sets the irrevocable to. (uopsigelig til) */
        DurationOngoing: boolean;
        /** Gets or sets the ongoing status. (løbende) */
        IrrevocableTo: Date;
        /** When the contract expires. (udløbet) */
        ExpirationDate: Date;
        /** Date the contract ends. (opsagt) */
        Terminated: Date;
        TerminationDeadlineId: number;
        /** Gets or sets the termination deadline option. (opsigelsesfrist) */
        TerminationDeadline: ITerminationDeadline;
        /** Gets or sets the payment milestones. */
        PaymentMilestones: Array<IPaymentMilestone>;
        OptionExtendId: number;
        OptionExtend: IOptionExtend;
        ExtendMultiplier: number;
        /** (løbende) */
        Running: string;
        /** (indtil udgangen af) */
        ByEnding: string;
        /** Gets or sets the handover trials. */
        HandoverTrials: Array<IHandoverTrial>;

        /** Gets or sets the operation remuneration begun. */
        OperationRemunerationBegun: Date;
        PaymentFreqencyId: number;
        PaymentFreqency: IPaymentFrequency;
        PaymentModelId: number;
        PaymentModel: IPaymentModel;
        PriceRegulationId: number;
        PriceRegulation: IPriceRegulation;

        /** The (local usages of) it systems, that this contract is associated to. */
        AssociatedSystemUsages: Array<IItContractItSystemUsage>;
        /** Number of system relations. */
        "AssociatedSystemRelations@odata.count": number;

        /** Gets or sets the intern economy streams. */
        InternEconomyStreams: Array<IEconomyStream>;
        /** Gets or sets the extern economy streams. */
        ExternEconomyStreams: Array<IEconomyStream>;

        /** Gets or sets the advices. */
        Advices: Array<IAdvice>;

        /** Whether the contract is active or not */
        IsActive: boolean;

        ReferenceId: number;
        Reference: IReference;
        AssociatedAgreementElementTypes: Array<IAssociatedAgreementElementTypes>;
    }
}
