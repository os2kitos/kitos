module Kitos.Models.ViewModel.ItContract {
    export interface ItContractOverviewViewModel extends Kitos.Models.ItContract.IItContractOverviewReadModel {
        roles: { [key: number]: Array<{ name: string, email: string }> }
    }
}