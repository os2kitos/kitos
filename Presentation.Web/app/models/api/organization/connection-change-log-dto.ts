module Kitos.Models.Api.Organization {
    export interface ConnectionChangeLogDTO {
        origin: ConnectionChangeLogOrigin
        user: Api.IUserWithEmail
        logTime: Date
        consequences: Array<ConnectionUpdateOrganizationUnitConsequenceDTO>
    }
}