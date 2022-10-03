module Kitos.Models.Api.Organization {
    export interface StsOrganizationAccessStatusResponseDTO {
        accessGranted: boolean
        error: CheckConnectionError | null
    }
}