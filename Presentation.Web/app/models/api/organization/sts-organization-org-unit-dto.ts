module Kitos.Models.Api.Organization {
    export interface StsOrganizationOrgUnitDTO {
        uuid : string
        name: string
        children: Array<StsOrganizationOrgUnitDTO>
    }
}