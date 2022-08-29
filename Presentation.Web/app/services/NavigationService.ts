module Kitos.Services {
    export class NavigationService {

        checkState = (state: string) => {
            const allowedStates = ["index", "organization.overview", "it-system.overview", "it-contract.overview", "data-processing.overview"];
            return allowedStates.indexOf(state) > -1;
        };
    }

    app.service("navigationService", NavigationService);
}