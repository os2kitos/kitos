
((ng, app) => {
    'use strict';

    app.directive("datePicker", [
        () => ({
            templateUrl: "app/shared/datePicker/datePicker.view.html",
            scope: {
                id: "@",
                model: "=ngModel",
                appendurl: "@",
                disabled: "=ngDisabled",
                required: "@"
            },
            link: ($scope) => {
                $scope.dateFormat = {
                    format: "dd-MM-yyyy"
                };
            }
        })
    ]);
})(angular, app);

