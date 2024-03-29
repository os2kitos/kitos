﻿module Kitos.Models.ItContract {

    /** Contains info about an it contract */
    export interface IItContract extends IEntity {
        /** Gets or sets the name. */
        Name: string;
        /** Gets or sets the note. */
        Note: string;
        /** Gets or sets it contract identifier defined by the user. */
        ItContractId: string;
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
        /** Gets or sets the chosen procurement strategy option identifier. (Genanskaffelsesstrategi) */
        ProcurementStrategyId: number;
        /** Gets or sets the chosen procurement strategy option. (Genanskaffelsesstrategi) */
        ProcurementStrategy: Models.OData.Generic.IOptionDTO<IItContract>;
        /** Gets or sets the procurement plan quarter. (genanskaffelsesplan) */
        ProcurementPlanQuarter: number;
        /** Gets or sets the procurement plan year. (genanskaffelsesplan) */
        ProcurementPlanYear: number;
        /** Gets or sets the chosen contract template identifier. */
        ContractTemplateId: number;
        /** Gets or sets the chosen contract template option. */
        ContractTemplate: Models.OData.Generic.IOptionDTO<IItContract>;
        /** Gets or sets the chosen contract type option identifier. */
        ContractTypeId: number;
        /** Gets or sets the chosen type of the contract. */
        ContractType: Models.OData.Generic.IOptionDTO<IItContract>;
        /** Gets or sets the chosen purchase form option identifier. */
        PurchaseFormId: number;
        /** Gets or sets the chosen purchase form option. */
        PurchaseForm: Models.OData.Generic.IOptionDTO<IItContract>;
        /** Id of parent ItContract */
        ParentId: number;
        /** The parent ItContract */
        Parent: IItContract;
        /** Gets or sets the contract children. */
        Children: Array<IItContract>;
        /** Gets or sets the chosen agreement elements. */
        AgreementElements: Array<Models.OData.Generic.IOptionDTO<IItContract>>;
        /** Gets or sets the chosen criticality. */
        Criticality: Models.OData.Generic.IOptionDTO<IItContract>,

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
        TerminationDeadline: Models.OData.Generic.IOptionDTO<IItContract>;
        OptionExtendId: number;
        OptionExtend: Models.OData.Generic.IOptionDTO<IItContract>;
        ExtendMultiplier: number;
        /** (løbende) */
        Running: YearSegmentOption;
        /** (indtil udgangen af) */
        ByEnding: YearSegmentOption;

        /** Gets or sets the operation remuneration begun. */
        OperationRemunerationBegun: Date;
        PaymentFreqencyId: number;
        PaymentFreqency: Models.OData.Generic.IOptionDTO<IItContract>;
        PaymentModelId: number;
        PaymentModel: Models.OData.Generic.IOptionDTO<IItContract>;
        PriceRegulationId: number;
        PriceRegulation: Models.OData.Generic.IOptionDTO<IItContract>;

        /** The (local usages of) it systems, that this contract is associated to. */
        AssociatedSystemUsages: Array<IItContractItSystemUsage>;
        /** Number of system relations. */
        AssociatedSystemRelations: Models.Odata.ItSystemUsage.ISystemRelationDTO[];

        /** Gets or sets the advices. */
        Advices: Array<IAdvice>;

        /** Whether the contract is active or not */
        IsActive: boolean;

        ReferenceId: number;
        Reference: IReference;
        AssociatedAgreementElementTypes: Array<IAssociatedAgreementElementTypes>;

        DataProcessingRegistrations: Array<{
            Id: number,
            Name: string,
            IsAgreementConcluded: string | null
        }>;
        ProcurementInitiated: Models.Api.Shared.YesNoUndecidedOption | null;
        Rights: Array<Models.IRightEntity<IItContract>>;
        ExternEconomyStreams: Array<IEconomyStream>;
    }
}
