module Kitos.Models.ViewModel.ItSystemUsage {

    export interface ILifeCycleStatusModel {
        id: string;
        text: string;
    }

    export class LifeCycleStatusViewModel {
        static readonly lifeCycleStatusOptions = Models.ItSystemUsage.LifeCycleStatusOptions.getAll();

        /*static getTextValueToTextMap() {
            return _.reduce(this.lifeCycleStatusOptions,
                (acc: any, current) => {
                    acc[current.textValue] = current.text;
                    return acc;
                },
                <any>{});
        }*/

    }
}