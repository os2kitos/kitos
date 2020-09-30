((ng, app) => {
    'use strict';

    app.directive("remarkField", [
        () => ({
            templateUrl: "app/shared/remarkField/remarkField.view.html",
            scope: {
                id: "@",
                model: "=ngModel",
                appendurl: "@",
                disabled: "=ngDisabled",
                required: "@",
                elementType: "=dataElementType"
            }
        })
    ]);
})(angular, app);