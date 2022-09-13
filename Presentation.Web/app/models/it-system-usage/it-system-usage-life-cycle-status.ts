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

    export class LifeCycleStatusFactory {
        static mapValueFromString(value: LifeCycleStatusType): ILifeCycleStatus {
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
    }

    export class LifeCycleStatusOptions {
        options: ILifeCycleStatus[];

        constructor() {
            this.options = Object.keys(LifeCycleStatusType)
                .filter(item => !isNaN(Number(item)))
                .map((value) => {
                    return LifeCycleStatusFactory.mapValueFromString(Number(value));
                }
            );
        }
    }
}