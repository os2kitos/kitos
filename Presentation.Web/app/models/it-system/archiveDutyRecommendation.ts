module Kitos.Models.ItSystem {
    export interface IArchiveDutyRecommendation {
        value: number;
        name: string;
    }

    export class ArchiveDutyRecommendationFactory {
        static mapFromNumeric(value: number): IArchiveDutyRecommendation {
            switch (value) {
                case 0:
                    return { name: "&nbsp;", value: value}; //Using html encoding for the option to
                case 1:
                    return { name: "B", value: value};
                case 2:
                    return { name: "K", value: value};
                case 3:
                    return { name: "Ingen vejledning", value: value};
                default:
                    return null;
            }
        }
    }

    export class ArchiveDutyRecommendationOptions {
        static getAll(): IArchiveDutyRecommendation[] {
            const results = new Array<IArchiveDutyRecommendation>();

            for (let i = 0; i <= 3; i++) {
                results.push(ArchiveDutyRecommendationFactory.mapFromNumeric(i));
            }

            return results;
        }
    }
}
