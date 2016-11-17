(function (ng, app) {
    'use strict';

    app.directive("selectMultipleOption", [
        function () {
            return {
                templateUrl: "app/shared/selectMultipleOption/selectMultipleOption.view.html",
                scope: {
                    id: "@",
                    placeholder: "@",
                    options: "&",
                    selectedIds: "=ngModel",
                    selectedTexts: "&",
                    autoSaveUrl: "@",
                    field: "@",
                    disabled: "&ngDisabled"
                },
                link: function (scope, element, attr, ctrl) {
                    var deletedOptions = [];
                    _.forEach(scope.selectedIds, function (value, key) {
                        var isDeleted = !_.some(scope.options(), { Id: value });
                        if (isDeleted) {
                            deletedOptions.push({ Id: value, Name: scope.selectedTexts()[key] });
                        }
                    });

                    scope.deletedOptions = deletedOptions;
                }
            };
        }
    ]);
})(angular, app);
