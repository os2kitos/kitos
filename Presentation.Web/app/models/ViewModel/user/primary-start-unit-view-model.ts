module Kitos.Models.ViewModel.User {
    export interface IPreferredStartUnitChoice {
        id: string
        name: string
        htmlSafeId: string
    }

    function createChoice(id: string, name: string): IPreferredStartUnitChoice {
        return {
            id: id,
            name: name,
            htmlSafeId: id.replace(".", "_")
        };
    }

    export const options: Array<IPreferredStartUnitChoice> = [
        createChoice(Kitos.Constants.ApplicationStateId.Index, "Start side"),
        createChoice(Kitos.Constants.ApplicationStateId.OrganizationOverview, "Organisation"),
        createChoice(Kitos.Constants.ApplicationStateId.SystemUsageOverview, "IT Systemer"),
        createChoice(Kitos.Constants.ApplicationStateId.SystemCatalog, "IT Systemkatalog"),
        createChoice(Kitos.Constants.ApplicationStateId.ContractOverview, "IT kontrakter"),
        createChoice(Kitos.Constants.ApplicationStateId.DataProcessingRegistrationOverview, "Databehandling")
    ];
}