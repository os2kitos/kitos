﻿module Kitos.Models.ItSystemUsage {

    export interface IItSystemUsageOverviewReadModel {
        Id: number;
        SourceEntityId: number;
        Name: string;
        ItSystemDisabled: boolean;
        IsActive: boolean;
        ItSystemUuid: string;
    }
}
