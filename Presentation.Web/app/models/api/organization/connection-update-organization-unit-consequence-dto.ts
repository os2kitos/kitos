module Kitos.Models.Api.Organization {
    export interface ConnectionUpdateOrganizationUnitConsequenceDTO {
        uuid : string;
        name : string;
        category: ConnectionUpdateOrganizationUnitChangeCategory;
        description : string;
    }
}