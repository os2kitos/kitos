module Kitos.Models.ViewModel.ItContract {

    export interface IItContractOverviewViewModel extends Models.ItContract.IItContract {
        Acquisition: number;
        Operation: number;
        Other: number;
        AuditDate: Date;
        status: {
            max: number;
            white: number;
            red: number;
            yellow: number;
            green: number;
        };
        roles: Array<any>;
    }
}