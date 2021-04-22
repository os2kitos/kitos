module Kitos.Models.ViewModel.ItSystemUsage {

    export interface IHostedAtModel {
        textValue: string;
        text: string;
    }

    export class HostedAtViewModel {
        static readonly hostedAtOptions = {
            UNDECIDED: <IHostedAtModel>{ textValue: "UNDECIDED", text: "" },
            ONPREMISE: <IHostedAtModel>{ textValue: "ONPREMISE", text: "On-premise" },
            EXTERNAL: <IHostedAtModel>{ textValue: "EXTERNAL", text: "Eksternt" },
        };

        static getTextValueToTextMap() {
            return _.reduce(this.hostedAtOptions,
                (acc: any, current) => {
                    acc[current.textValue] = current.text;
                    return acc;
                },
                <any>{});
        }

    }
}