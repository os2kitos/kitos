module Kitos.Models.ItSystemUsage {
    export interface IArchiveDuty {
        value: number;
        name: string;
    }

    export class ArchiveDutyFactory {
        static mapFromNumeric(value: number): IArchiveDuty {
            switch (value) {
                case 0:
                    return { name: "&nbsp;", value: value}; //Using html encoding for the option to
                case 1:
                    return { name: "B", value: value};
                case 2:
                    return { name: "K", value: value};
                case 3:
                    return { name: "Ved ikke", value: value};
                default:
                    return null;
            }
        }
    }

    export class ArchiveDutyOptions {
        static getAll(): IArchiveDuty[] {
            const results = new Array<IArchiveDuty>();

            for (let i = 0; i <= 3; i++) {
                results.push(ArchiveDutyFactory.mapFromNumeric(i));
            }

            return results;
        }
    }
}
