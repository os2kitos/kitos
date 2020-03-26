module Kitos.Models.ViewModel.ItSystemUsage {

    export interface IArchiveDutyModel {
        textValue: string;
        text: string;
    }

    export class ArchiveDutyViewModel {
        static readonly archiveDuties = {
            Undecided: <IArchiveDutyModel>{ textValue: "Undecided", text: "Ikke besluttet" },
            B: <IArchiveDutyModel>{ textValue: "B", text: "B" },
            K: <IArchiveDutyModel>{ textValue: "K", text: "K" },
            Unknown: <IArchiveDutyModel>{ textValue: "Unknown", text: "Ved ikke" },
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