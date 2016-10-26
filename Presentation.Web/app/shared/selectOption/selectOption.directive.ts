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
                   //var foundSelectedInOptions = _.find(scope.options(), function(option: any) { return option.Id === scope.selectedId });
                   //scope.isDeletedSelected = scope.selectedId != null && !foundSelectedInOptions;

                    scope.isDeletedSelected = false;

                    scope.savedId = scope.selectedId;

                    scope.$watch('selectedId', function (value) {
                        var foundSelectedInOptions = _.find(scope.options(), function (option: any) { return option.Id === parseInt(scope.selectedId, 10) });

                        if (foundSelectedInOptions) {
                            scope.optionDescription = foundSelectedInOptions.Description;
                        }
                    });
                }
            };
        }
    ]);
})(angular, app);
