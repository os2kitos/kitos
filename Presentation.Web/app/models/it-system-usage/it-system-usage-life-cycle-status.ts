module Kitos.Models.ItSystemUsage {
    export enum LifeCycleStatusType {
        Undecided = 0,
        NotInUse = 1,
        PhasingIn = 2,
        Operational = 3,
        PhasingOut = 4
    }

    export interface ILifeCycleStatus {
        id: number;
        text: string;
    }

    export class LifeCycleStatusOptions {
        options: ILifeCycleStatus[];

        mapValueFromEnum(value: LifeCycleStatusType): ILifeCycleStatus {
            switch (value) {
            case LifeCycleStatusType.Undecided:
                return { text: Constants.Select2.EmptyField, id: value };
            case LifeCycleStatusType.NotInUse:
                return { text: "Under indfasning", id: value };
            case LifeCycleStatusType.PhasingIn:
                return { text: "I drift", id: value };
            case LifeCycleStatusType.Operational:
                return { text: "Under udfasning", id: value };
            case LifeCycleStatusType.PhasingOut:
                return { text: "Ikke i drift", id: value };
            default:
                return null;
            }
        }

        mapValueFromString(value: string): string {
            switch (value) {
                case "Undecided":
                    return Constants.Select2.EmptyField;
                case "NotInUse":
                    return "Under indfasning";
                case "PhasingIn":
                    return "I drift";
                case "Operational":
                    return "Under udfasning";
                case "PhasingOut":
                    return "Ikke i drift";
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