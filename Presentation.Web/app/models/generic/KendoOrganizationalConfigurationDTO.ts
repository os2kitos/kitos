module Kitos.Models.Generic {
    export enum OverviewType {
        ItSystemUsage = 0
    }

    export interface IKendoOrganizationalConfigurationDTO {
        orgId: number,
        overviewType: OverviewType,
        visibleColumnsCsv: string,
        version: number
    }

}