module Kitos.Models.ViewModel.ItSystemUsage {
    import Select2OptionViewModel = ViewModel.Generic.Select2OptionViewModel;

    export enum SensitiveDataLevel {
        NONE = 0,
        PERSONALDATA = 1,
        PERSONALDATANDSENSITIVEDATA = 2,
        PERSONALLEGALDATA = 3
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
        showPersonalData: boolean;
        showSensitivePersonalData: boolean;
    }

    export class SystemUsageViewModel implements ISystemUsageViewModel {
        showPersonalData: boolean;
        showSensitivePersonalData: boolean;
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
            this.showPersonalData = _.some(this.sensitiveDataLevels,
                x => x === SensitiveDataLevel.PERSONALDATA || x === SensitiveDataLevel.PERSONALDATANDSENSITIVEDATA);
            this.showSensitivePersonalData = _.some(this.sensitiveDataLevels,
                x => x === SensitiveDataLevel.PERSONALDATANDSENSITIVEDATA);
        }

        mapDataLevels(dataLevel: any) : SensitiveDataLevel {
            switch (dataLevel.dataSensitivityLevel) {
                case 0:
                    return SensitiveDataLevel.NONE;
                case 1:
                    return SensitiveDataLevel.PERSONALDATA;
                case 2:
                    return SensitiveDataLevel.PERSONALDATANDSENSITIVEDATA;
                case 3:
                    return SensitiveDataLevel.PERSONALLEGALDATA;
                default:
                    throw new RangeError(`${dataLevel.dataSensitivityLevel} is not a valid SensitiveDataLevel`);
            }
        }


    }
}