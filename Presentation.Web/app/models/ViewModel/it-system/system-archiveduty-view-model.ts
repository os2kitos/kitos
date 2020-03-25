module Kitos.Models.ViewModel.ItSystem {

    export interface IArchiveDutyRecommendationModel {
        value: number;
        textValue: string;
        text: string;
    }

    export class ArchiveDutyRecommendationViewModel {
        static readonly archiveDuties = {
            Undecided: <IArchiveDutyRecommendationModel>{ value: 0, textValue: "Undecided", text: "Ikke besluttet" },
            B: <IArchiveDutyRecommendationModel>{ value: 1, textValue: "B", text: "B" },
            K: <IArchiveDutyRecommendationModel>{ value: 2, textValue: "K", text: "K" },
            NoRecommendation: <IArchiveDutyRecommendationModel>{ value: 3, textValue: "NoRecommendation", text: "Ingen vejledning" },
        };

        static getTextValueToTextMap() {
            return _.reduce(this.archiveDuties,
                (acc: any, current) => {
                    acc[current.textValue] = current.text;
                    return acc;
                },
                <any>{});
        }
    }
}