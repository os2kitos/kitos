module Kitos.Models.ViewModel.ItSystem {

    export interface IArchiveDutyRecommendationModel {
        textValue: string;
        text: string;
    }

    export class ArchiveDutyRecommendationViewModel {
        static readonly archiveDuties = {
            Undecided: <IArchiveDutyRecommendationModel>{ textValue: "Undecided", text: "Ikke besluttet" },
            B: <IArchiveDutyRecommendationModel>{ textValue: "B", text: "B" },
            K: <IArchiveDutyRecommendationModel>{ textValue: "K", text: "K" },
            NoRecommendation: <IArchiveDutyRecommendationModel>{ textValue: "NoRecommendation", text: "Ingen vejledning" },
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