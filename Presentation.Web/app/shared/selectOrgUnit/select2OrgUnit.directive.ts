(function (ng, app) {
    'use strict';

    app.controller("select2OrgUnitController", [
        "$scope",
        "select2LoadingService",
        ($scope: any, select2LoadingService: Kitos.Services.Select2LoadingService) => {

            const options: Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<Kitos.Models.Api.Organization.OrganizationUnit>[] = $scope.options;
            if ($scope.renderUnitOriginIndication === true) {
                const externalUnits = $scope.options.filter(x => x.optionalObjectContext?.externalOriginUuid !== null);
                if (externalUnits.length > 0) {
                    $scope.hasExternalUnits = true;
                    $scope.select2Config = select2LoadingService.select2LocalDataFormatted(() => options, unit => Kitos.Helpers.Select2OptionsFormatHelper.formatIndentation(unit, true), $scope.allowClear);
                    return;
                }
            }

            $scope.select2Config = select2LoadingService.select2LocalDataFormatted(() => options, unit => Kitos.Helpers.Select2OptionsFormatHelper.formatIndentation(unit, false), $scope.allowClear);
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
                    onChange: "&",
                    renderUnitOriginIndication: "="
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