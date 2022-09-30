module Kitos.Models.ViewModel.FeatureToggle {
    export interface IFeatureToggleViewModel {
        featureToggle: string | null;
    }

    export class FeatureToggleViewModelFactory {

        constructor(private readonly $) {

        }

        createFromViewState() {
            let featureToggleValue: string | null = null;
            const state = new Utility.ViewDataState(this.$);

            const featureToggleElement = state.getStateOrNull("feature-toggle");
            if (featureToggleElement) {
                featureToggleValue = featureToggleElement.value;
                featureToggleElement.element.remove();
            }

            return <IFeatureToggleViewModel>{
                featureToggle: featureToggleValue
            };
        }
    }
}