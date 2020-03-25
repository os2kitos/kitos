module Kitos.Models.ViewModel.ItSystemUsage {

    export interface IArchiveDutyModel {
        value: number;
        textValue: string;
        text: string;
    }

    export class ArchiveDutyViewModel {
        static readonly archiveDuties = {
            Undecided: <IArchiveDutyModel>{ value: 0, textValue: "Undecided", text: "Ikke besluttet" },
            B: <IArchiveDutyModel>{ value: 1, textValue: "B", text: "B" },
            K: <IArchiveDutyModel>{ value: 2, textValue: "K", text: "K" },
            Unknown: <IArchiveDutyModel>{ value: 3, textValue: "Unknown", text: "Ved ikke" },
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