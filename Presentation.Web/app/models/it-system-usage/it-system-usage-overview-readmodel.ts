module Kitos.Models.ItSystemUsage {

    export interface IItSystemUsageOverviewReadModel {
        Id: number;
        SourceEntityId: number;
        Name: string;
        ItSystemDisabled: boolean;
        IsActive: boolean;
        ItSystemUuid: string;
        LocalSystemId: string;
        Version: string;
        LocalCallName: string;
        ParentItSystemId: number;
        ParentItSystemName: string;
    }
}
