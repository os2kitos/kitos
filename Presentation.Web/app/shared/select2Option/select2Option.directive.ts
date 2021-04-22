(function (ng, app) {
    'use strict';

    app.controller("select2OptionController", ["$scope", "entityMapper", "select2LoadingService", function ($scope, entityMapper, select2LoadingService) {
        var options = entityMapper.mapOptionToSelect2ViewModel($scope.options);

        var foundSelectedInOptions = _.find(options, function (option: any) {
            if ($scope.selectedId === undefined || $scope.selectedId === null) {
                return false;
            }
            if ($scope.selectedId.id === undefined) {
                return option.id == $scope.selectedId;
            }
            else {
                return option.id == $scope.selectedId.id;
            }
        });
        if ($scope.options && $scope.selectedId != null && !foundSelectedInOptions) {
            options.splice(0, 0, { id: $scope.selectedId, text: $scope.selectedText + " (Slettes)", optionalObjectContext: null });
        }

        $scope.select2Config = select2LoadingService.select2LocalDataNoSearch(() => options, true);
    }]);

    app.directive("select2Option", [
        function () {
            return {
                templateUrl: "app/shared/select2Option/select2Option.view.html",
                scope: {
                    id: "@",
                    label: "@",
                    placeholder: "@",
                    options: "=",
                    selectedId: "=ngModel",
                    selectedText: "@",
                    autoSaveUrl: "@",
                    field: "@",
                    disabled: "=ngDisabled",
                    required: "@"
                },
                controller: "select2OptionController",
                link: function (scope, element, attr, ctrl) {
                   
                    scope.optionDescription = null;

                    scope.$watch('selectedId', function (value) {
                        var foundSelectedInOptions = _.find(scope.options, function (option: any) {
                            if (scope.selectedId != null) {
                                return option.Id === parseInt(scope.selectedId.id, 10);
                            }
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