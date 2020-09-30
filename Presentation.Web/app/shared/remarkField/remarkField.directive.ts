((ng, app) => {
    'use strict';

    app.directive("remarkField", [
        () => ({
            templateUrl: "app/shared/remarkField/remarkField.view.html",
            scope: {
                model: "=ngModel",
                appendurl: "@",
                disabled: "=ngDisabled",
                required: "@"
            }
        })
    ]);
})(angular, app);