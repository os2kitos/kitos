(function (ng, app) {
    'use strict';

    app.directive("helpText", [
        function () {
            return {
                templateUrl: "app/shared/helpText/helpText.view.html",
                scope: {
                    id: "@",
                },
                controller: [
                    '$scope', '$http', '$uibModal', function ($scope, $http, $uibModal) {
                        var parent = $scope;
                        $scope.showHelpTextModal = function() {

                        var modalInstance = $uibModal.open({
                            windowClass: "modal fade in",
                            templateUrl: "app/shared/helpText/helpTextModal.view.html",
                            controller: ["$scope", "$uibModalInstance", "notify", function ($scope, $modalInstance, nofity) {
                                $http.get("odata/HelpTexts(" + parent.id + ")")
                                    .success((result: any) => {
                                        $scope.title = result.Title;
                                        $scope.description = result.Description;
                                    })
                            }]
                        });
                    }
                    }
                ]
            };
        }
    ]);
})(angular, app);
