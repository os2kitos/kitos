module Kitos.Models.ItSystem {
    export interface IArchiveDutyRecommendation {
        id: string;
        text: string;
    }

    export class ArchiveDutyRecommendationFactory {
        static mapFromNumeric(value: number): IArchiveDutyRecommendation {
            switch (value) {
                case 0:
                    return { text: Kitos.Constants.Select2.EmptyField, id: "0" }; //Using html encoding for the option to
                case 1:
                    return { text: "B", id: "1"};
                case 2:
                    return { text: "K", id: "2"};
                case 3:
                    return { text: "Ingen vejledning", id: "3"};
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
