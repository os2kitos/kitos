module Kitos.Models.Api.Organization {
    export interface ConnectionChangeLogDTO {
        id: number;
        origin: ConnectionChangeLogOrigin;
        user: Api.IUserWithEmail;
        logTime: Date;
        consequences: Array<ConnectionUpdateOrganizationUnitConsequenceDTO>;
    }
}