module Kitos.Services {
    export class NavigationService {

        checkState = (state: string) => {
            const allowedStates = ["index", "organization.overview", "it-project.overview", "it-system.overview", "it-contract.overview", "reports.overview","data-processing.overview"];
            return allowedStates.indexOf(state) > -1;
        };
    }

    app.service("navigationService", NavigationService);
}