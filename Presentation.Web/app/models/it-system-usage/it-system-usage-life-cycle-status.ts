module Kitos.Models.ItSystemUsage {
    export interface ILifeCycleStatus {
        id: number;
        text: string;
    }

    export class LifeCycleStatusFactory {
        static mapFromNumeric(value: number): IArchiveDuty {
            switch (value) {
            case 0:
                return { text: Constants.Select2.EmptyField, id: value };
            case 1:
                return { text: "Under indfasning", id: value };
            case 2:
                return { text: "I drift", id: value };
            case 3:
                return { text: "Under udfasning", id: value };
            case 4:
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
    }
}