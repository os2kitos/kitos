(function (ng, app) {
    'use strict';

    app.controller("selectSimpleController", ["$scope", "select2LoadingService", function ($scope, select2LoadingService) {


        $scope.select2Config = select2LoadingService.select2LocalDataNoSearch(() => $scope.options, true);
    }]);

    app.directive("selectSimple", [
        function () {
            return {
                templateUrl: "app/shared/selectSimple/selectSimple.view.html",
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
                    required: "@"
                },
                controller: "selectSimpleController",
                link: function (scope, element, attr, ctrl) {

                    if (scope.function != null) {
                        scope.$watch('selected', (newVal) => {
                            if (newVal === undefined) {
                                return;
                            }
                            if (newVal === null) {
                                scope.function(null);
                            }
                            else {
                                scope.function(newVal.id);
                            }
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