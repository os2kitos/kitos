((ng, app) => {
    'use strict';

    app.directive("singleSelectSelect2", [
        () => ({
            templateUrl: "app/shared/singleSelect/singleSelectSelect2.view.html",
            scope: {
                id: "@",
                placeholder: "@",
                model: "=ngModel",
                appendurl: "@",
                disabled: "=ngDisabled",
                required: "@"
            }
        })
    ]);
})(angular, app);
