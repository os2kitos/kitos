module Kitos.Models.ViewModel.ItSystemUsage {
    import Select2OptionViewModel = ViewModel.Generic.Select2OptionViewModel;

    export enum SensitiveDataLevel {
        NONE = 0,
        PERSONALDATA = 1,
        SENSITIVEDATA = 2,
        LEGALDATA = 3
    }

    export enum DataOption {
        NO = 0,
        YES = 1,
        DONTKNOW = 2,
        UNDECIDED = 3
    }

    export enum RiskLevel {
        LOW = 0,
        MIDDLE = 1,
        HIGH = 2,
        UNDECIDED = 3
    }

    export class DataOptions {
        options: Select2OptionViewModel[];
        constructor() {
            this.options = [
                <Select2OptionViewModel>{ id: 3, text: "&nbsp" },
                <Select2OptionViewModel>{ id: 1, text: "Ja" },
                <Select2OptionViewModel>{ id: 0, text: "Nej" },
                <Select2OptionViewModel>{ id: 2, text: "Ved ikke" }
            ];
        }
    }

    export class RiskLevelOptions {
        options: Select2OptionViewModel[];
        constructor() {
            this.options = [
                <Select2OptionViewModel>{ id: 3, text: "&nbsp" },
                <Select2OptionViewModel>{ id: 0, text: "Lav risiko" },
                <Select2OptionViewModel>{ id: 1, text: "Mellem risiko" },
                <Select2OptionViewModel>{ id: 2, text: "Høj risiko" }
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
        isBusinessCritical: DataOption;
        dataProcessorControl: DataOption;
        precautions: DataOption;
        userSupervision: DataOption;
        riskAssessment: DataOption;
        preRiskAssessment: RiskLevel;
        DPIA: DataOption;
        answeringDataDPIA: DataOption;
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
        isBusinessCritical: DataOption;
        dataProcessorControl: DataOption;
        precautions: DataOption;
        userSupervision: DataOption;
        riskAssessment: DataOption;
        preRiskAssessment: RiskLevel;
        DPIA: DataOption;
        answeringDataDPIA: DataOption;

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

            this.isBusinessCritical = this.mapDataOption(itSystemUsage.isBusinessCritical);
            this.dataProcessorControl = this.mapDataOption(itSystemUsage.dataProcessorControl);
            this.precautions = this.mapDataOption(itSystemUsage.precautions);
            this.userSupervision = this.mapDataOption(itSystemUsage.userSupervision);
            this.riskAssessment = this.mapDataOption(itSystemUsage.riskAssessment);
            this.preRiskAssessment = this.mapRiskLevelOption(itSystemUsage.preRiskAssessment);
            this.DPIA = this.mapDataOption(itSystemUsage.dpia);
            this.answeringDataDPIA = this.mapDataOption(itSystemUsage.answeringDataDPIA);

        }

        mapDataLevels(dataLevel: any): SensitiveDataLevel {
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

        mapDataOption(dataOption: number): DataOption {
            switch (dataOption) {
                case 0:
                    return DataOption.NO;
                case 1:
                    return DataOption.YES;
                case 2:
                    return DataOption.DONTKNOW;
                case 3:
                    return DataOption.UNDECIDED;
                default:
                    throw new RangeError(`${dataOption} is not a valid SensitiveDataLevel`);
            }
        }

        mapRiskLevelOption(dataOption: number): RiskLevel {
            switch (dataOption) {
            case 0:
                    return RiskLevel.LOW;
            case 1:
                    return RiskLevel.MIDDLE;
            case 2:
                    return RiskLevel.HIGH;
            case 3:
                    return RiskLevel.UNDECIDED;
            default:
                throw new RangeError(`${dataOption} is not a valid RiskLevel`);
            }
        }


    }
}