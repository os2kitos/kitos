module Kitos.Models.ItSystemUsage.Migration {

    import NamedEntityDTO = Models.Generic.NamedEntity.NamedEntityDTO;

    export interface IItContractItSystemUsageDTO {
        contract: NamedEntityDTO;
        systemAssociatedInContract: boolean;
        affectedInterfaceUsages: Array<NamedEntityDTO>;
        interfaceExhibitUsagesToBeDeleted: Array<NamedEntityDTO>;
    }
}