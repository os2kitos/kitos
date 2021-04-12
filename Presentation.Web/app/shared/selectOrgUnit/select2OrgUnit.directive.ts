(function (ng, app) {
    'use strict';

    app.controller("select2OrgUnitController", [
        "$scope",
        "select2LoadingService",
        ($scope: any, select2LoadingService: Kitos.Services.Select2LoadingService) => {
            $scope.select2Config = select2LoadingService.select2LocalDataFormatted(() => $scope.options, formatResults, $scope.allowClear);

            function formatResults(result: Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<any>): string {
                function visit(text: string, indentationLevel: number): string {
                    if (indentationLevel <= 0) {
                        return text;
                    }
                    //indentation is four non breaking spaces
                    return visit("&nbsp&nbsp&nbsp&nbsp" + text, indentationLevel - 1);
                }

                var formattedResult = visit(result.text, result.indentationLevel);
                return formattedResult;
            }
    }]);

    app.directive("select2OrgUnit", [
        function () {
            return {
                templateUrl: "app/shared/selectOrgUnit/select2OrgUnit.view.html",
                scope: {
                    id: "@",
                    label: "@",
                    placeholder: "@",
                    options: "=",
                    selected: "=ngModel",
                    function: "=",
                    autoSaveUrl: "@",
                    field: "@",
                    disabled: "=ngDisabled",
                    allowClear: "="
                },
                controller: "select2OrgUnitController",
                link: function (scope, element, attr, ctrl) {

                    if (scope.function != null) {
                        scope.$watch('selected', (newVal) => {
                            if (newVal === undefined) {
                                return;
                            }
                            scope.function(newVal);
                        });
                    }
                    

                    scope.$watch(attr.disabled, function (newVal) {
                        element.prop('disabled', newVal);
                    });
                }
            };
        }
    ]);
})(angular, app);