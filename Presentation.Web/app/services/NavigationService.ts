
module Kitos.Services {

    export class NavigationService {
        checkState = (state: string) => {
            const allowedStates = Kitos.Models.ViewModel.User.options.map(option => option.id);
            return allowedStates.indexOf(state) > -1;
        };
    }

    app.service("navigationService", NavigationService);
}