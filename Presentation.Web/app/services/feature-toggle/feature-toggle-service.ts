module Kitos.Services.FeatureToggle {

    export interface IFeatureToggleService {
        hasFeature(feature: TemporaryFeature): boolean
        addFeature(feature: TemporaryFeature): void
    }

    class FeatureToggleService implements IFeatureToggleService {
        private readonly _ftState: Array<TemporaryFeature>;

        constructor() {
            this._ftState = [];
        }

        hasFeature(feature: TemporaryFeature): boolean {
            return this._ftState.indexOf(feature) !== -1;
        }

        addFeature(feature: TemporaryFeature): void {
            this._ftState.push(feature);
        }
    }

    app.constant("featureToggleService", new FeatureToggleService());

}
