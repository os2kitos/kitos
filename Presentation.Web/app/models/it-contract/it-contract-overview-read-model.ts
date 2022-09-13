module Kitos.Models.ItContract {

    export interface IItContractOverviewRoleAssignmentReadModel {
        RoleId: number
        UserId: number
        UserFullName: string
        Email: string
    }

    export interface IItContractOverviewReadModel {
        SourceEntityId: number
        Name: string
        IsActive: boolean
        ContractId: string | null
        RoleAssignments: Array<IItContractOverviewRoleAssignmentReadModel>
        ParentContractName: string | null
        ParentContractId: number | null
        CriticalityId: number | null
        ResponsibleOrgUnitName: string
        ResponsibleOrgUnitId: number | null
        ContractTypeId: number | null
        ContractTemplateId: number | null
        PurchaseFormId: number | null
        ProcurementStrategyId: number | null
        ProcurementPlanYear: number | null
        ProcurementPlanQuarter: number | null
        ProcurementInitiated: Models.Api.Shared.YesNoUndecidedOption | null
        DataProcessingAgreementsCsv: string
        DataProcessingAgreements: Array<{
            DataProcessingRegistrationId: number,
            DataProcessingRegistrationName: string
        }>
        ItSystemUsagesCsv: string
        ItSystemUsagesSystemUuidCsv: string
        ItSystemUsages: Array<{
            ItSystemUsageId: number
            ItSystemUsageName: string
        }>
        NumberOfAssociatedSystemRelations: number
        ActiveReferenceTitle: string | null
        ActiveReferenceUrl: string | null
        ActiveReferenceExternalReferenceId: string | null
        AccumulatedAcquisitionCost : number
        AccumulatedOperationCost : number
        AccumulatedOtherCost: number
        OperationRemunerationBegunDate: Date | null
        PaymentModelId: number | null
        PaymentFrequencyId: number | null
        LatestAuditDate: Date | null
        AuditStatusWhite : number
        AuditStatusRed : number
        AuditStatusYellow : number
        AuditStatusGreen: number
        OptionExtendId: number | null
        TerminationDeadlineId: number | null
        IrrevocableTo: Date | null
        TerminatedAt: Date | null
        LastEditedByUserName: string
        LastEditedAtDate: Date
        Concluded: Date | null
        ExpirationDate : Date | null
    }
}
