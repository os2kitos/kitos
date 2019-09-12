module Kitos.Models.ItSystemUsage.Migration {

    import NamedEntityDTO = Models.Generic.NamedEntity.NamedEntityDTO

    export interface IItSystemUsageMigration {
        targetUsage: NamedEntityDTO;
        fromSystem: NamedEntityDTO;
        toSystem: NamedEntityDTO;
        affectedItProjects: Array<NamedEntityDTO>;
        affectedContracts: Array<IItContractItSystemUsageDTO>;
    }

    export interface IItContractItSystemUsageDTO {
        contract: NamedEntityDTO;
        systemAssociatedInContract: boolean;
        affectedInterfaceUsages: Array<NamedEntityDTO>;
        interfaceExhibitUsagesToBeDeleted: Array<NamedEntityDTO>;
    }

}