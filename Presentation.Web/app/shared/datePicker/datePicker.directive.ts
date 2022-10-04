
((ng, app) => {
    'use strict';

    app.directive("datePicker", [
        () => ({
            templateUrl: "app/shared/datePicker/datePicker.view.html",
            scope: {
                model: "=ngModel",
                appendurl: "@",
                disabled: "=ngDisabled",
                required: "@"
            },
            link: ($scope) => {
                $scope.dateFormat = Kitos.Configs.standardKendoDatePickerOptions;
            }
        })
    ]);
})(angular, app);

