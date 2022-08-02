module Kitos.Models.ItContract {

    export interface IItContractDTO {
        id: number,
        name: string,
        dataProcessingRegistrations: Models.Generic.NamedEntity.NamedEntityDTO[];
    }

    export interface IItContractOptions {
        //TODO: Let's get the roles in here too
        criticalityOptions: Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO[];
        contractTypeOptions: Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO[];
        contractTemplateOptions: Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO[];
        purchaseFormOptions: Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO[];
        procurementStrategyOptions: Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO[];
        paymentModelOptions: Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO[];
        paymentFrequencyOptions: Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO[];
        optionExtendOptions: Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO[];
        terminationDeadlineOptions: Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO[];
    }

    export interface IContractProcurementPlanDTO {
        procurementPlanYear: number,
        procurementPlanQuarter: number,
    }
}