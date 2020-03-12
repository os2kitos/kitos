module Kitos.Models.ItSystemUsage.Migration {

    import NamedEntityDTO = Models.Generic.NamedEntity.NamedEntityDTO;

    export interface RelationMigrationDTO {
        toSystemUsage: NamedEntityDTO;
        fromSystemUsage: NamedEntityDTO;
        description: string;
        interface: NamedEntityDTO;
        frequencyType: NamedEntityDTO;
        contract: NamedEntityDTO;
    }
}