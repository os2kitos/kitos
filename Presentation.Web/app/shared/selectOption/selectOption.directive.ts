(function (ng, app) {
    'use strict';

    app.directive("selectOption", [
        function () {
            return {
                templateUrl: "app/shared/selectOption/selectOption.view.html",
                scope: {
                    id: "@",
                    label: "@",
                    placeholder: "@",
                    options: "=",
                    selectedId: "=ngModel",
                    selectedText: "@",
                    autoSaveUrl: "@",
                    appendurl: "@",
                    field: "@",
                    disabled: "=ngDisabled",
                    required: "@"
                },
                link: function (scope, element, attr, ctrl) {
                    var foundSelectedInOptions = _.find(scope.options, function(option: any) { return option.Id == scope.selectedId });
                    if (scope.options && scope.selectedId != null && !foundSelectedInOptions) {
                        scope.options.splice(0, 0, { Id: scope.selectedId, Name: scope.selectedText + " (Slettes)" });
                    }

                    scope.savedId = scope.selectedId;
                    scope.optionDescription = null;

                    scope.$watch('selectedId', function (value) {
                        var foundSelectedInOptions = _.find(scope.options, function (option: any) {
                            return option.Id === parseInt(scope.selectedId, 10);
                        });

                        if (foundSelectedInOptions) {
                            scope.optionDescription = foundSelectedInOptions.Description;
                        } else {
                            scope.optionDescription = null;
                        }
                    });

                    scope.$watch(attr.disabled, function (newVal) {
                        element.prop('disabled', newVal);
                    });
                }
            };
        }
    ]);
})(angular, app);
