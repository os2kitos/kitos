module Kitos.Models.ViewModel.Sso {
    export interface ISsoStateViewModel {
        show: boolean;
        error: string;
    }

    export class SsoStateViewModelFactory {

        constructor(private readonly $) {

        }

        createFromViewState() {
            let show = false;
            let error = null;

            const state = new Utility.ViewDataState(this.$);

            const toggle = state.getStateOrNull("feature-toggle");
            if (toggle && toggle.value === "Sso") {
                show = true;
            }

            const ssoError = state.getStateOrNull("sso-error");
            if (ssoError) {
                error = ssoError.value;
                ssoError.element.remove();
            }

            return <ISsoStateViewModel>{
                show: show,
                error: error
            };
        }
    }
}