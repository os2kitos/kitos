((ng, app) => {
    'use strict';

    app.directive("remarkField", [
        () => ({
            templateUrl: "app/shared/remarkField/remarkField.view.html",
            scope: {
                id: "@",
                rows: "@",
                model: "=ngModel",
                disabled: "=ngDisabled",
            }
        })
    ]);
})(angular, app);