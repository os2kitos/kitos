module Kitos.Services {
    export class NavigationService {

        checkState = (state: string) => {
            const allowedStates = [
                Constants.ApplicationStateId.Index,
                Constants.ApplicationStateId.OrganizationOverview,
                Constants.ApplicationStateId.SystemUsageOverview,
                Constants.ApplicationStateId.SystemCatalog,
                Constants.ApplicationStateId.ContractOverview,
                Constants.ApplicationStateId.DataProcessingRegistrationOverview
            ];
            return allowedStates.indexOf(state) > -1;
        };
    }

    app.service("navigationService", NavigationService);
}