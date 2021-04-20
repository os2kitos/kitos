module Kitos.Models.ItSystemUsage {
    export interface IItSystemUsageOverviewOptionsDTO {
        businessTypes: Array<Models.Generic.NamedEntity.NamedEntityDTO>,
        systemRoles: Array<Models.Generic.Roles.BusinessRoleDTO>,
        organizationUnits: Array<Models.Generic.Hierarchy.HierarchyNodeDTO>,
    }
}
