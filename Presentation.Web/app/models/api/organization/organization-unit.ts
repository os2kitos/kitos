module Kitos.Models.Api.Organization {
    export interface OrganizationUnit extends Models.Generic.NamedEntity.NamedEntityDTO{
        children: OrganizationUnit[];
        ean: number;
        localId: number;
        parentId: number;
        organizationId: number;
    }
}