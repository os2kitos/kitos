module Kitos.Models.Api.Organization {
    export interface StsOrganizationSynchronizationStatusResponseDTO {
        accessStatus: StsOrganizationAccessStatusResponseDTO
        connected: boolean
        subscribesToUpdates: boolean
        dateOfLatestCheckBySubscription : Date | null
        synchronizationDepth: number | null
        canCreateConnection: boolean
        canUpdateConnection: boolean
        canDeleteConnection: boolean
    }
}