module Kitos.Models.ViewModel.ItSystemUsage {
    import Select2OptionViewModel = ViewModel.Generic.Select2OptionViewModel;

    export enum SensitiveDataLevel {
        NONE = 0,
        PERSONALDATA = 1,
        SENSITIVEDATA = 2,
        LEGALDATA = 3
    }

    export class SensitiveDataLevelOptions {
        options: Select2OptionViewModel[];
        constructor() {
            this.options = [
                <Select2OptionViewModel>{ id: 0, text: "Ingen persondata" },
                <Select2OptionViewModel>{ id: 1, text: "Almindelige persondata" },
                <Select2OptionViewModel>{ id: 2, text: "Følsomme persondata" },
                <Select2OptionViewModel>{ id: 3, text: "Straffedomme og lovovertrædelser" }
            ];
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
        sensitiveDataLevels: SensitiveDataLevel[];
        personalLegalDataSelected: boolean;
        personalSensitiveDataSelected: boolean;
        personalRegularDataSelected: boolean;
        personalNoDataSelected: boolean;
    }

    export class SystemUsageViewModel implements ISystemUsageViewModel {
        personalLegalDataSelected: boolean;
        personalSensitiveDataSelected: boolean;
        personalRegularDataSelected: boolean;
        personalNoDataSelected: boolean;
        sensitiveDataLevels: SensitiveDataLevel[];
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
            this.sensitiveDataLevels = _.map(itSystemUsage.sensitiveDataLevels, this.mapDataLevels);
            this.personalNoDataSelected = _.some(this.sensitiveDataLevels, x => x === SensitiveDataLevel.NONE);
            this.personalRegularDataSelected = _.some(this.sensitiveDataLevels, x => x === SensitiveDataLevel.PERSONALDATA);
            this.personalSensitiveDataSelected = _.some(this.sensitiveDataLevels, x => x === SensitiveDataLevel.SENSITIVEDATA);
            this.personalLegalDataSelected = _.some(this.sensitiveDataLevels, x => x === SensitiveDataLevel.LEGALDATA)

        }

        mapDataLevels(dataLevel: any) : SensitiveDataLevel {
            switch (dataLevel.dataSensitivityLevel) {
                case 0:
                    return SensitiveDataLevel.NONE;
                case 1:
                    return SensitiveDataLevel.PERSONALDATA;
                case 2:
                    return SensitiveDataLevel.SENSITIVEDATA;
                case 3:
                    return SensitiveDataLevel.LEGALDATA;
                default:
                    throw new RangeError(`${dataLevel.dataSensitivityLevel} is not a valid SensitiveDataLevel`);
            }
        }


    }
}