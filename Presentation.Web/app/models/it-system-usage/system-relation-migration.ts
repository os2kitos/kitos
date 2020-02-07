module Kitos.Models.ItSystemUsage.Migration {

    import NamedEntityDTO = Models.Generic.NamedEntity.NamedEntityDTO;

    export interface RelationMigrationDTO {
        sourceSystem: NamedEntityDTO;
        targetSystem: NamedEntityDTO;
        description: string;
        interface: NamedEntityDTO;
        contract: NamedEntityDTO;
    }
}