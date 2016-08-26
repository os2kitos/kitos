(function(ng, app) {
    'use strict';

    app.directive("selectOption", [
        function() {
            return {
                templateUrl: "app/shared/selectOption/selectOption.view.html",
                scope: {
                    id: "@",
                    label: "@",
                    placeholder: "@",
                    options: "&",
                    selectedId: "=ngModel",
                    selectedText: "@",
                    autoSaveUrl: "@",
                    appendurl: "@",
                    field: "@",
                    disabled: "&ngDisabled",
                },
                link: function (scope, element, attr, ctrl) {
                    var foundSelectedInOptions = _.find(scope.options(), function(option: any) { return option.id === scope.selectedId });
                    scope.isDeletedSelected = scope.selectedId != null && !foundSelectedInOptions;

                    scope.savedId = scope.selectedId;
                }
            };
        }
    ]);
})(angular, app);
