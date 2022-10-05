module Kitos.Models.Api.Organization {
    export interface CheckStsOrganizationConnectionResponseDTO {
        connected: boolean
        error: CheckConnectionError | null
    }
}