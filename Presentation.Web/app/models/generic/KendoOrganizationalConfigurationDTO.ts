module Kitos.Models.Generic {
    export enum OverviewType {
        ItSystemUsage = 0
    }

    export interface IKendoOrganizationalConfigurationDTO {
        orgId: number,
        overviewType: OverviewType,
        columns: IKendoColumnConfigurationDTO[],
        version: string
    }

    export interface IKendoColumnConfigurationDTO {
        persistId: string,
        index: number,
        hidden: boolean
    }
}