module Kitos.Models.Generic.Hierarchy {

    export interface HierarchyNodeDTO extends Generic.NamedEntity.NamedEntityDTO {
        parentId: number;
    }
}