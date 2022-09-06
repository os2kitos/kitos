module Kitos.Models.Generic {
    export enum OverviewType {
        ItSystemUsage = 0,
        ItContract = 1
    }

    export interface IKendoOrganizationalConfigurationDTO {
        orgId: number,
        overviewType: OverviewType,
        visibleColumns: IKendoColumnConfigurationDTO[],
        version: string
    }

    export interface IKendoColumnConfigurationDTO {
        persistId: string,
        index: number
    }
}