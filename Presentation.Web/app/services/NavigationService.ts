module Kitos.Services {

    export class NavigationService {
        private static readonly allowedStates = Kitos.Models.ViewModel.User.options.map(option => option.id);

        checkState = (state: string) => {
            return NavigationService.allowedStates.indexOf(state) > -1;
        };
    }

    app.service("navigationService", NavigationService);
}