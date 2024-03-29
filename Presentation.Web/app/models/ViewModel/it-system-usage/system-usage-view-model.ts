﻿module Kitos.Models.ViewModel.ItSystemUsage {
    import UpdatedSelect2OptionViewModel = ViewModel.Generic.UpdatedSelect2OptionViewModel;
    import ISelectedValueViewModel = ViewModel.Generic.ISelectedValueViewModel;
    import PersonalDataOption = Models.Api.ItSystemUsage.PersonalDataOption;

    export interface IPersonalDataModel extends ISelectedValueViewModel<PersonalDataOption> {
        checked: boolean;
    }

    export enum DataOption {
        NO = "0",
        YES = "1",
        DONTKNOW = "2",
        UNDECIDED = "3"
    }

    export enum RiskLevel {
        LOW = "0",
        MIDDLE = "1",
        HIGH = "2",
        UNDECIDED = "3"
    }

    export enum HostedAt {
        UNDECIDED = "0",
        ONPREMISE = "1",
        EXTERNAL = "2",
    }

    export class DataOptions {
        options: UpdatedSelect2OptionViewModel<any>[];
        constructor() {
            this.options = [
                <UpdatedSelect2OptionViewModel<any>>{ id: DataOption.UNDECIDED, text: Kitos.Constants.Select2.EmptyField },
                <UpdatedSelect2OptionViewModel<any>>{ id: DataOption.YES, text: "Ja" },
                <UpdatedSelect2OptionViewModel<any>>{ id: DataOption.NO, text: "Nej" },
                <UpdatedSelect2OptionViewModel<any>>{ id: DataOption.DONTKNOW, text: "Ved ikke" }
            ];
        }
    }

    export class RiskLevelOptions {
        options: UpdatedSelect2OptionViewModel<any>[];
        constructor() {
            this.options = [
                <UpdatedSelect2OptionViewModel<any>>{ id: RiskLevel.UNDECIDED, text: Kitos.Constants.Select2.EmptyField },
                <UpdatedSelect2OptionViewModel<any>>{ id: RiskLevel.LOW, text: "Lav risiko" },
                <UpdatedSelect2OptionViewModel<any>>{ id: RiskLevel.MIDDLE, text: "Mellem risiko" },
                <UpdatedSelect2OptionViewModel<any>>{ id: RiskLevel.HIGH, text: "Høj risiko" }
            ];
        }

    }


    export class SensitiveDataLevelViewModel {
        static readonly levels = {
            none: <ISelectedValueViewModel<number>>{ value: 0, textValue: "NONE", text: "Ingen personoplysninger" },
            personal: <ISelectedValueViewModel<number>>{ value: 1, textValue: "PERSONALDATA", text: "Almindelige personoplysninger" },
            sensitive: <ISelectedValueViewModel<number>>{ value: 2, textValue: "SENSITIVEDATA", text: "Følsomme personoplysninger" },
            legal: <ISelectedValueViewModel<number>>{ value: 3, textValue: "LEGALDATA", text: "Straffedomme og lovovertrædelser" },
        };

        static readonly levelOrder = {
            "NONE": 0,
            "PERSONALDATA": 1,
            "SENSITIVEDATA": 2,
            "LEGALDATA": 3
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

    export class PersonalDataViewModel {
        private readonly data = [
            { value: PersonalDataOption.CprNumber, textValue: "CPR", text: "CPR-nr" } as ISelectedValueViewModel<PersonalDataOption>,
            { value: PersonalDataOption.SocialProblems, textValue: "SocialProblems", text: "Væsentlige sociale problemer" } as ISelectedValueViewModel<PersonalDataOption>,
            { value: PersonalDataOption.OtherPrivateMatters, textValue: "OtherPrivate", text: "Andre rent private forhold" } as ISelectedValueViewModel<PersonalDataOption>,
        ] as ISelectedValueViewModel<PersonalDataOption>[];

        options: IPersonalDataModel[];

        constructor(values: PersonalDataOption[]) {
            this.options = this.data.map(option => {
                var value = values.some(x => x === option.value);
                return {
                    value: option.value,
                    text: option.text,
                    textValue: option.textValue,
                    checked: value
                } as IPersonalDataModel;
            });
        }
    }

    export class HostedAtOptions {
        static readonly options = [
            <UpdatedSelect2OptionViewModel<string>>{ id: HostedAt.UNDECIDED, optionalObjectContext: "UNDECIDED", text: Kitos.Constants.Select2.EmptyField },
            <UpdatedSelect2OptionViewModel<any>>{ id: HostedAt.ONPREMISE, optionalObjectContext: "ONPREMISE", text: "On-premise" },
            <UpdatedSelect2OptionViewModel<any>>{ id: HostedAt.EXTERNAL, optionalObjectContext: "EXTERNAL", text: "Eksternt" }
        ]

        static getoptionalObjectContextToTextMap() {
            return _.reduce(this.options,
                (acc: any, current) => {
                    acc[current.optionalObjectContext] = current.text;
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
        lifeCycleStatus: Models.ItSystemUsage.LifeCycleStatusType;
        activeAccordingToValidityPeriod: boolean;
        activeAccordingToLifeCycle: boolean;
        active: boolean;
        legalDataSelected: boolean;
        sensitiveDataSelected: boolean;
        personalDataSelected: boolean;
        personalData: PersonalDataOption[];
        noDataSelected: boolean;
        isBusinessCritical: DataOption;
        precautions: DataOption;
        userSupervision: DataOption;
        riskAssessment: DataOption;
        preRiskAssessment: RiskLevel;
        DPIA: DataOption;
        answeringDataDPIA: DataOption;
        hostedAt: HostedAt;
        lastChanged: string;
        lastChangedBy: string;
    }

    export class SystemUsageViewModel implements ISystemUsageViewModel {
        legalDataSelected: boolean;
        sensitiveDataSelected: boolean;
        personalDataSelected: boolean;
        personalData: PersonalDataOption[];
        noDataSelected: boolean;
        id: number;
        organizationId: number;
        itSystem: ViewModel.ItSystem.ISystemViewModel;
        concluded: any;
        expirationDate: any;
        lifeCycleStatus: Models.ItSystemUsage.LifeCycleStatusType;
        activeAccordingToValidityPeriod: boolean;
        activeAccordingToLifeCycle: boolean;
        active: boolean;
        isBusinessCritical: DataOption;
        precautions: DataOption;
        userSupervision: DataOption;
        riskAssessment: DataOption;
        preRiskAssessment: RiskLevel;
        DPIA: DataOption;
        answeringDataDPIA: DataOption;
        hostedAt: HostedAt;
        userCount: string;
        lastChanged: string;
        lastChangedBy: string;

        constructor(itSystemUsage: any) {
            this.id = itSystemUsage.id;
            this.organizationId = itSystemUsage.organizationId;
            this.itSystem = new ViewModel.ItSystem.SystemViewModel(itSystemUsage.itSystem);
            this.concluded = itSystemUsage.concluded;
            this.expirationDate = itSystemUsage.expirationDate;
            this.lifeCycleStatus = itSystemUsage.lifeCycleStatus;
            this.activeAccordingToValidityPeriod = itSystemUsage.activeAccordingToValidityPeriod;
            this.active = itSystemUsage.active;
            this.lastChanged = Helpers.RenderFieldsHelper.renderDate(itSystemUsage.lastChanged);
            this.lastChangedBy = `${itSystemUsage.lastChangedByUserName} ${itSystemUsage.lastChangedByUserLastName}`;
            const sensitiveDataLevels = _.map(itSystemUsage.sensitiveDataLevels, this.mapDataLevels);
            this.noDataSelected = _.some(sensitiveDataLevels, x => x === SensitiveDataLevelViewModel.levels.none.value);
            this.personalDataSelected = _.some(sensitiveDataLevels, x => x === SensitiveDataLevelViewModel.levels.personal.value);
            this.sensitiveDataSelected = _.some(sensitiveDataLevels, x => x === SensitiveDataLevelViewModel.levels.sensitive.value);
            this.legalDataSelected = _.some(sensitiveDataLevels, x => x === SensitiveDataLevelViewModel.levels.legal.value);
            this.personalData = itSystemUsage.personalData;

            this.isBusinessCritical = this.mapDataOption(itSystemUsage.isBusinessCritical);
            this.precautions = this.mapDataOption(itSystemUsage.precautions);
            this.userSupervision = this.mapDataOption(itSystemUsage.userSupervision);
            this.riskAssessment = this.mapDataOption(itSystemUsage.riskAssessment);
            this.preRiskAssessment = this.mapRiskLevelOption(itSystemUsage.preRiskAssessment);
            this.DPIA = this.mapDataOption(itSystemUsage.dpia);
            this.answeringDataDPIA = this.mapDataOption(itSystemUsage.answeringDataDPIA);
            this.hostedAt = this.mapHostedAtOption(itSystemUsage.hostedAt);
            this.userCount = itSystemUsage.userCount == null ? null : itSystemUsage.userCount.toString();
        }

        mapHostedAtOption(hostedAtOption: number) {
            switch (hostedAtOption) {
                case null:
                    return HostedAt.UNDECIDED;
                case 0:
                    return HostedAt.UNDECIDED;
                case 1:
                    return HostedAt.ONPREMISE;
                case 2:
                    return HostedAt.EXTERNAL;
                default:
                    throw new RangeError(`${hostedAtOption} is not a valid hostedAt Option`);
            }
        }

        mapDataLevels(dataLevel: any): number {
            return dataLevel.dataSensitivityLevel;
        }

        mapDataOption(dataOption: number): DataOption {
            switch (dataOption) {
                case null:
                    return DataOption.UNDECIDED;
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
                case null:
                    return RiskLevel.UNDECIDED;
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