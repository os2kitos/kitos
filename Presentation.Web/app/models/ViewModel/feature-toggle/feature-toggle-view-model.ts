module Kitos.Models.ViewModel.FeatureToggle {
    export interface IFeatureToggleViewModel {
        featureToggle: Kitos.Services.FeatureToggle.TemporaryFeature | null;
    }

    export class FeatureToggleViewModelFactory {

        constructor(private readonly $) {

        }

        createFromViewState() {
            let featureToggleValue: Kitos.Services.FeatureToggle.TemporaryFeature | null = null;
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