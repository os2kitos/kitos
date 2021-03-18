module Kitos.Models.ItSystemUsage {
    export interface IArchiveDuty {
        id: number;
        text: string;
    }

    export class ArchiveDutyFactory {
        static mapFromNumeric(value: number): IArchiveDuty {
            switch (value) {
                case 0:
                    return { text: "\u00a0", id: value }; //Using non-breaking space character
                case 1:
                    return { text: "B", id: value };
                case 2:
                    return { text: "K", id: value };
                case 3:
                    return { text: "Ved ikke", id: value };
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
