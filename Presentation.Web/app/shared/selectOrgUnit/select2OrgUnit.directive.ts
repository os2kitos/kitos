(function (ng, app) {
    'use strict';

    app.controller("select2OrgUnitController", [
        "$scope",
        "select2LoadingService",
        ($scope: any, select2LoadingService: Kitos.Services.Select2LoadingService) => {
            $scope.select2Config = select2LoadingService.select2LocalDataFormatted(() => $scope.options, Kitos.Helpers.Select2OptionsFormatHelper.formatIndentation, $scope.allowClear);
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
                    allowClear: "=",
                    onChange: "&"
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