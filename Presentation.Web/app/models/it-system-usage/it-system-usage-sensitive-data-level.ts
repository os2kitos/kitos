module Kitos.Models.ItSystemUsage {
    export interface IItSystemUsageSensitiveDataLevel {
        Id: number;

        ItSystemUsage: IItSystemUsage;

        SensitivityDataLevel: Models.ViewModel.ItSystemUsage.SensitiveDataLevel;
    }
}