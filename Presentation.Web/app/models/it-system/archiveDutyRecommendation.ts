module Kitos.Models.ItSystem {
    export interface IArchiveDutyRecommendation {
        id: number;
        text: string;
    }

    export class ArchiveDutyRecommendationFactory {
        static mapFromNumeric(value: number): IArchiveDutyRecommendation {
            switch (value) {
                case 0:
                    return { text: "\u00a0", id: value}; //Using html encoding for the option to
                case 1:
                    return { text: "B", id: value};
                case 2:
                    return { text: "K", id: value};
                case 3:
                    return { text: "Ingen vejledning", id: value};
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
