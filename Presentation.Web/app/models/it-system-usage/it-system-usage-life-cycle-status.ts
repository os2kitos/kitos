module Kitos.Models.ItSystemUsage {
    export enum LifeCycleStatusType {
        Undecided = 0,
        NotInUse = 1,
        PhasingIn = 2,
        Operational = 3,
        PhasingOut = 4
    }

    export class LifeCycleStatusOptions {
        options: Models.ViewModel.Generic.Select2OptionViewModel<LifeCycleStatusType>[];

        mapValueFromEnum(value: LifeCycleStatusType): Models.ViewModel.Generic.Select2OptionViewModel<LifeCycleStatusType> {
            switch (value) {
                case LifeCycleStatusType.Undecided:
                    return { text: Constants.Select2.EmptyField, id: value, optionalObjectContext: value };
                case LifeCycleStatusType.NotInUse:
                    return { text: Constants.LifeCycleStatus.NotInUseDescription, id: value, optionalObjectContext: value };
                case LifeCycleStatusType.PhasingIn:
                    return { text: Constants.LifeCycleStatus.PhasingInDescription, id: value, optionalObjectContext: value };
                case LifeCycleStatusType.Operational:
                    return { text: Constants.LifeCycleStatus.OperationalDescription, id: value, optionalObjectContext: value };
                case LifeCycleStatusType.PhasingOut:
                    return { text: Constants.LifeCycleStatus.PhasingOutDescription, id: value, optionalObjectContext: value };
                default:
                    return null;
            }
        }

        mapValueFromString(value: string): string {
            switch (value) {
                case Constants.LifeCycleStatus.UndecidedTitle:
                    return Constants.Select2.EmptyField;
                case Constants.LifeCycleStatus.NotInUseTitle:
                    return Constants.LifeCycleStatus.NotInUseDescription;
                case Constants.LifeCycleStatus.PhasingInTitle:
                    return Constants.LifeCycleStatus.PhasingInDescription;
                case Constants.LifeCycleStatus.OperationalTitle:
                    return Constants.LifeCycleStatus.OperationalDescription;
                case Constants.LifeCycleStatus.PhasingOutTitle:
                    return Constants.LifeCycleStatus.PhasingOutDescription;
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