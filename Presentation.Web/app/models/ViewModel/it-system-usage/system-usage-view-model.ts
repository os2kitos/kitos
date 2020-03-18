module Kitos.Models.ViewModel.ItSystemUsage {

    export interface ISensitiveDataLevelModel {
        value: number;
        textValue: string;
        text: string;
    }


    export class SensitiveDataLevelViewModel {
        static readonly levels = {
            none: <ISensitiveDataLevelModel>{ value: 0, textValue: "NONE", text: "Ingen persondata" },
            personal: <ISensitiveDataLevelModel>{ value: 1, textValue: "PERSONALDATA", text: "Almindelige persondata" },
            sensitive: <ISensitiveDataLevelModel>{ value: 2, textValue: "SENSITIVEDATA", text: "Følsomme persondata" },
            legal: <ISensitiveDataLevelModel>{ value: 3, textValue: "LEGALDATA", text: "Straffedomme og lovovertrædelser" },
        };

        static getTextValueToTextMap() {
            return _.reduce(this.levels,
                (acc: any, current) => {
                    acc[current.textValue] = current.text;

                    return acc;
                },
                <any>{});
        }
        
    }


    export interface ISystemUsageViewModel {
        id: number;
        organizationId: number;
        itSystem: ViewModel.ItSystem.ISystemViewModel;
        concluded;
        expirationDate;
        isActive: boolean;
        active: boolean;
        legalDataSelected: boolean;
        sensitiveDataSelected: boolean;
        personalDataSelected: boolean;
        noDataSelected: boolean;
    }

    export class SystemUsageViewModel implements ISystemUsageViewModel {
        legalDataSelected: boolean;
        sensitiveDataSelected: boolean;
        personalDataSelected: boolean;
        noDataSelected: boolean;
        id: number;
        organizationId: number;
        itSystem: ViewModel.ItSystem.ISystemViewModel;
        concluded: any;
        expirationDate: any;
        isActive: boolean;
        active: boolean;

        constructor(itSystemUsage: any) {
            this.id = itSystemUsage.id;
            this.organizationId = itSystemUsage.organizationId;
            this.itSystem = new ViewModel.ItSystem.SystemViewModel(itSystemUsage.itSystem);
            this.concluded = itSystemUsage.concluded;
            this.expirationDate = itSystemUsage.expirationDate;
            this.isActive = itSystemUsage.isActive;
            this.active = itSystemUsage.active;

            const sensitiveDataLevels = _.map(itSystemUsage.sensitiveDataLevels, this.mapDataLevels);
            this.noDataSelected = _.some(sensitiveDataLevels, x => x === SensitiveDataLevelViewModel.levels.none.value);
            this.personalDataSelected = _.some(sensitiveDataLevels, x => x === SensitiveDataLevelViewModel.levels.personal.value);
            this.sensitiveDataSelected = _.some(sensitiveDataLevels, x => x === SensitiveDataLevelViewModel.levels.sensitive.value);
            this.legalDataSelected = _.some(sensitiveDataLevels, x => x === SensitiveDataLevelViewModel.levels.legal.value);

        }

        mapDataLevels(dataLevel: any): number {
            return dataLevel.dataSensitivityLevel;
        }

    }
}