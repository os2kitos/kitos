module Kitos.Models.ViewModel.ItSystemUsage {
    import Select2OptionViewModel = ViewModel.Generic.Select2OptionViewModel;

    export enum SensitiveDataLevel {
        NONE = 0,
        PERSONALDATA = 1,
        PERSONALDATANDSENSITIVEDATA = 2,
        PERSONALLEGALDATA = 3
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

    export enum Operation {
        UNDECIDED = 0,
        ONPREMISE = 1,
        EXTERNAL = 2,
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

    export class OperationOptions {
        options: Select2OptionViewModel[];
        constructor() {
            this.options = [
                <Select2OptionViewModel>{ id: 0, text: "&nbsp" },
                <Select2OptionViewModel>{ id: 1, text: "On-premise" },
                <Select2OptionViewModel>{ id: 2, text: "Eksternt" }
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
        operation: Operation;
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
        operation: Operation;

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
            this.personalSensitiveDataSelected = _.some(this.sensitiveDataLevels, x => x === SensitiveDataLevel.PERSONALDATANDSENSITIVEDATA);
            this.personalLegalDataSelected = _.some(this.sensitiveDataLevels, x => x === SensitiveDataLevel.PERSONALLEGALDATA);

            this.isBusinessCritical = this.mapDataOption(itSystemUsage.isBusinessCritical);
            this.dataProcessorControl = this.mapDataOption(itSystemUsage.dataProcessorControl);
            this.precautions = this.mapDataOption(itSystemUsage.precautions);
            this.userSupervision = this.mapDataOption(itSystemUsage.userSupervision);
            this.riskAssessment = this.mapDataOption(itSystemUsage.riskAssessment);
            this.preRiskAssessment = this.mapRiskLevelOption(itSystemUsage.preRiskAssessment);
            this.DPIA = this.mapDataOption(itSystemUsage.dpia);
            this.answeringDataDPIA = this.mapDataOption(itSystemUsage.answeringDataDPIA);
            this.operation = this.mapOperationOption(itSystemUsage.operation);

        }

        mapOperationOption(operationOption: number) {
            switch (operationOption) {
                case 0:
                    return Operation.UNDECIDED;
                case 1:
                    return Operation.ONPREMISE;
                case 2:
                    return Operation.EXTERNAL;
                default:
                    throw new RangeError(`${operationOption} is not a valid operation Option`);
            }
        }

        mapDataLevels(dataLevel: any): SensitiveDataLevel {
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