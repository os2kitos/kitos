module Kitos.Models.ItSystemUsage {

    export enum LifeCycleStatusType {
        Undecided = 0,
        NotInUse = 1,
        PhasingIn = 2,
        Operational = 3,
        PhasingOut = 4
    }

    interface ILifeCycleOption {
        text: string;
        enumAsString: string;
        enumValue: LifeCycleStatusType;
    }


    class LifeCycleStatus {
        static getRange(): Array<ILifeCycleOption> {
            const options: Array<ILifeCycleOption> = [];

            options.push(
                {
                    text: Constants.Select2.EmptyField,
                    enumAsString: this.undecided.enumStringValue,
                    enumValue: this.undecided.enumValue
                });
            options.push(
                {
                    text: this.phasingIn.text,
                    enumAsString: this.phasingIn.enumStringValue,
                    enumValue: this.phasingIn.enumValue
                });
            options.push(
                {
                    text: this.operational.text,
                    enumAsString: this.operational.enumStringValue,
                    enumValue: this.operational.enumValue
                });
            options.push(
                {
                    text: this.phasingOut.text,
                    enumAsString: this.phasingOut.enumStringValue,
                    enumValue: this.phasingOut.enumValue
                });
            options.push(
                {
                    text: this.notInUse.text,
                    enumAsString: this.notInUse.enumStringValue,
                    enumValue: this.notInUse.enumValue
                });

            return options;
        }


        static readonly undecided = {
            text: Constants.Select2.EmptyField,
            enumStringValue: "Undecided",
            enumValue: LifeCycleStatusType.Undecided
        }

        static readonly phasingIn = {
            text: "Under indfasning",
            enumStringValue: "PhasingIn",
            enumValue: LifeCycleStatusType.PhasingIn
        }

        static readonly operational = {
            text: "I drift",
            enumStringValue: "Operational",
            enumValue: LifeCycleStatusType.Operational
        }

        static readonly phasingOut = {
            text: "Under udfasning",
            enumStringValue: "PhasingOut",
            enumValue: LifeCycleStatusType.PhasingOut
        }

        static readonly notInUse = {
            text: "Ikke i drift",
            enumStringValue: "NotInUse",
            enumValue: LifeCycleStatusType.NotInUse
        }
    }

    export class LifeCycleStatusOptions {
        options: Models.ViewModel.Generic.Select2OptionViewModel<LifeCycleStatusType>[];
        private readonly enumStringToTextMap: Record<string, string>;
        
        mapValueFromString(value: string): string {
            return this.enumStringToTextMap[value] ?? "";
        }

        constructor() {
            const optionLookup = LifeCycleStatus.getRange().reduce<{ [key: number]: ILifeCycleOption }>((acc, next) => {
                acc[next.enumValue] = next;
                return acc;
            }, {});

            this.options = Object
                .keys(LifeCycleStatusType)
                .filter(item => !isNaN(Number(item)))
                .map((value) => {
                    const enumValue = Number(value);
                    const option = optionLookup[enumValue];
                    return {
                        text: option.text,
                        id: enumValue,
                        optionalObjectContext: enumValue
                    } as Models.ViewModel.Generic.Select2OptionViewModel<LifeCycleStatusType>;
                }
                );

            this.enumStringToTextMap = LifeCycleStatus.getRange().reduce((record, item) => {
                record[item.enumAsString] = item.text;
                return record;
            }, {} as Record<string, string>);
        }
    }
}