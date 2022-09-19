module Kitos.Services {
    const allowedStates = Kitos.Models.ViewModel.User.options.map(option => option.id);

    export class NavigationService {

        checkState = (state: string) => {
            return allowedStates.indexOf(state) > -1;
        };
    }

    app.service("navigationService", NavigationService);
}