module Kitos.Models.ItSystemUsage.Migration {

    import NamedEntityDTO = Models.Generic.NamedEntity.NamedEntityDTO;

    export interface IItSystemUsageMigrationDTO {
        targetUsage: NamedEntityDTO;
        fromSystem: NamedEntityDTO;
        toSystem: NamedEntityDTO;
        affectedItProjects: Array<NamedEntityDTO>;
        affectedContracts: Array<NamedEntityDTO>;
        affectedRelations: Array<RelationMigrationDTO>;
    }
}