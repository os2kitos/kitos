(function (ng, app) {
    'use strict';

    app.directive("selectMultipleOption", [
        function () {
            return {
                templateUrl: "app/shared/selectMultipleOption/selectMultipleOption.view.html",
                scope: {
                    id: "@",
                    label: "@",
                    placeholder: "@",
                    options: "&",
                    selectedId: "=ngModel",
                    selectedText: "@",
                    autoSaveUrl: "@",
                    field: "@",
                    disabled: "&ngDisabled"
                },
                link: function (scope, element, attr, ctrl) {
                    var foundSelectedInOptions = _.find(scope.options(), function (option) { return option.id === scope.selectedId });
                    scope.isDeletedSelected = scope.selectedId != null && !foundSelectedInOptions;

                    scope.savedId = scope.selectedId;
                }
            };
        }
    ]);
})(angular, app);
