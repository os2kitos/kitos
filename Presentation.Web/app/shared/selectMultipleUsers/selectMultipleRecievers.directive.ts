(function (ng, app) {
    'use strict';

    app.directive("selectMultipleRecievers", [
        function () {
            return {
                templateUrl: "app/shared/selectMultipleUsers/selectMultipleRecievers.view.html",
                scope: {
                    id: "@",
                    placeholder: "@",
                    roles: "&",
                    users: "&",
                    selectedIds: "=ngModel",
                    selectedTexts: "&",
                    field: "@",
                    disabled: "&ngDisabled"
                }
            };
        }
    ]);
})(angular, app);
