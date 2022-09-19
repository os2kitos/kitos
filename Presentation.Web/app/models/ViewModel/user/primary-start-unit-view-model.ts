module Kitos.Models.ViewModel.User {
    export const options: Array<{ id: string, name: string }> = [
        {
            id: Kitos.Constants.ApplicationStateId.Index,
            name: "Start side"
        },
        {
            id: Kitos.Constants.ApplicationStateId.OrganizationOverview,
            name: "Organisation"
        },
        {
            id: Kitos.Constants.ApplicationStateId.SystemUsageOverview,
            name: "IT Systemer"
        },
        {
            id: Kitos.Constants.ApplicationStateId.SystemCatalog,
            name: "IT Systemkatalog"
        },
        {
            id: Kitos.Constants.ApplicationStateId.ContractOverview,
            name: "IT kontrakter"
        },
        {
            id: Kitos.Constants.ApplicationStateId.DataProcessingRegistrationOverview,
            name: "Databehandling"
        },
    ];
}