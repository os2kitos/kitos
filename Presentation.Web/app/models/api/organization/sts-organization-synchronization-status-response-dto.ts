module Kitos.Models.Api.Organization {
    export interface StsOrganizationSynchronizationStatusResponseDTO {
        accessStatus: StsOrganizationAccessStatusResponseDTO
        connected: boolean
        subscribesToUpdates: boolean
        synchronizationDepth: number | null
        canCreateConnection: boolean
        canUpdateConnection: boolean
        canDeleteConnection: boolean
    }
}