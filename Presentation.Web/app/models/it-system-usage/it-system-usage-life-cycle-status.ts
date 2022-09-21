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

    export class LifeCycleStatusOptions {
        readonly options: Models.ViewModel.Generic.Select2OptionViewModel<LifeCycleStatusType>[];
        private readonly enumStringToTextMap: Record<string, string>;
        
        mapValueFromString(value: string): string {
            return this.enumStringToTextMap[value] ?? "";
        }

        constructor() {

            this.enumStringToTextMap = {};
            this.options = [];

            //Decides the available range and order of options presented to the user.
            const optionRange: Array<ILifeCycleOption> = [
                {
                    text: Constants.Select2.EmptyField,
                    enumAsString: "Undecided",
                    enumValue: LifeCycleStatusType.Undecided
                },
                {
                    text: "Under indfasning",
                    enumAsString: "PhasingIn",
                    enumValue: LifeCycleStatusType.PhasingIn
                },
                {
                    text: "I drift",
                    enumAsString: "Operational",
                    enumValue: LifeCycleStatusType.Operational
                },
                {
                    text: "Under udfasning",
                    enumAsString: "PhasingOut",
                    enumValue: LifeCycleStatusType.PhasingOut
                },
                {
                    text: "Ikke i drift",
                    enumAsString: "NotInUse",
                    enumValue: LifeCycleStatusType.NotInUse
                }
            ];

            //**********************************************//
            //Setup the fixed option range and lookup table*//
            //**********************************************//
            optionRange.forEach(option => {
                //Add the option
                this.options.push({
                    text: option.text,
                    id: option.enumValue,
                    optionalObjectContext: option.enumValue
                });

                //Extend the enumStringToTextMap
                this.enumStringToTextMap[option.enumAsString] = option.text;
            });
        }
    }
}