module Kitos.Models.ViewModel.Sso {
    export interface ISsoStateViewModel {
        show: boolean;
        error: string;
        startPreference: string;
    }

    export class SsoStateViewModelFactory {

        constructor(private readonly $) {

        }

        createFromViewState() {
            let error = null;
            let startPreference = null;
            const state = new Utility.ViewDataState(this.$);

            const ssoError = state.getStateOrNull("sso-error");
            if (ssoError) {
                error = ssoError.value;
                ssoError.element.remove();
            }

            const ssoStartPreference = state.getStateOrNull("sso-start-preference");
            if (ssoStartPreference) {
                startPreference = ssoStartPreference.value;
                ssoStartPreference.element.remove();
            }

            return <ISsoStateViewModel>{
                show: true,
                error: error,
                startPreference: startPreference
            };
        }
    }
}