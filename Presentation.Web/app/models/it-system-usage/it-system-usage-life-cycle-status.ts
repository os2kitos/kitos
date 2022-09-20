module Kitos.Models.ItSystemUsage {
    export enum LifeCycleStatusType {
        Undecided = 0,
        NotInUse = 1,
        PhasingIn = 2,
        Operational = 3,
        PhasingOut = 4
    }
    
    export class LifeCycleStatus {
        static readonly Undecided = {
            Text: Constants.Select2.EmptyField,
            EnumStringValue: "Undecided"
        }

        static readonly PhasingIn = {
            Text: "Under indfasning",
            EnumStringValue: "PhasingIn"
        }

        static readonly Operational = {
            Text: "I drift",
            EnumStringValue: "Operational"
        }

        static readonly PhasingOut = {
            Text: "Under udfasning",
            EnumStringValue: "PhasingOut"
        }

        static readonly NotInUse = {
            Text: "Ikke i drift",
            EnumStringValue: "NotInUse"
        }
    }

    export class LifeCycleStatusOptions {
        options: Models.ViewModel.Generic.Select2OptionViewModel<LifeCycleStatusType>[];

        mapValueFromEnum(value: LifeCycleStatusType): Models.ViewModel.Generic.Select2OptionViewModel<LifeCycleStatusType> {
            switch (value) {
                case LifeCycleStatusType.Undecided:
                    return { text: LifeCycleStatus.Undecided.Text, id: value, optionalObjectContext: value };
                case LifeCycleStatusType.PhasingIn:
                    return { text: LifeCycleStatus.PhasingIn.Text, id: value, optionalObjectContext: value };
                case LifeCycleStatusType.Operational:
                    return { text: LifeCycleStatus.Operational.Text, id: value, optionalObjectContext: value };
                case LifeCycleStatusType.PhasingOut:
                    return { text: LifeCycleStatus.PhasingOut.Text, id: value, optionalObjectContext: value };
                case LifeCycleStatusType.NotInUse:
                    return { text: LifeCycleStatus.NotInUse.Text, id: value, optionalObjectContext: value };
                default:
                    return null;
            }
        }

        mapValueFromString(value: string): string {
            switch (value) {
                case LifeCycleStatus.Undecided.EnumStringValue:
                    return LifeCycleStatus.Undecided.Text;
                case LifeCycleStatus.PhasingIn.EnumStringValue:
                    return LifeCycleStatus.PhasingIn.Text;
                case LifeCycleStatus.Operational.EnumStringValue:
                    return LifeCycleStatus.Operational.Text;
                case LifeCycleStatus.PhasingOut.EnumStringValue:
                    return LifeCycleStatus.PhasingOut.Text;
                case LifeCycleStatus.NotInUse.EnumStringValue:
                    return LifeCycleStatus.NotInUse.Text;
                default:
                    return "";
            }
        }

        constructor() {
            this.options = Object.keys(LifeCycleStatusType)
                .filter(item => !isNaN(Number(item)))
                .map((value) => {
                    return this.mapValueFromEnum(Number(value));
                }
            );
        }
    }
}