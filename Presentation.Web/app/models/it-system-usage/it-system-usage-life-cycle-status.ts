module Kitos.Models.ItSystemUsage {
    export enum LifeCycleStatusType {
        Undecided = 0,
        NotInUse = 1,
        PhasingIn = 2,
        Operational = 3,
        PhasingOut = 4
    }

    /*export interface ILifeCycleStatus {
        id: number;
        text: string;
    }

    export class LifeCycleStatusFactory {
        static mapmap(value: LifeCycleStatusType): IArchiveDuty {
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
        static getAll(): ILifeCycleStatus[] {
            const results = new Array<ILifeCycleStatus>();

            for (let i = 0; i <= 4; i++) {
                results.push(LifeCycleStatusFactory.mapFromNumeric(i));
            }

            return results;
        }
    }*/
}