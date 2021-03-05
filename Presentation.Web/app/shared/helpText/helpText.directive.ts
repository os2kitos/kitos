(function (ng, app) {
    'use strict';

    app.directive("helpText", [
        function () {
            return {
                templateUrl: "app/shared/helpText/helpText.view.html",
                scope: {
                    key: "@",
                    defaultTitle: "@",
                    noButtonLayout: "@"
                },
                controller: [
                    '$scope', '$http', '$uibModal', '$sce', function ($scope, $http, $uibModal, $sce) {
                        var parent = $scope;

                        $scope.showHelpTextModal = function () {
                            var modalInstance = $uibModal.open({
                                windowClass: "modal fade in",
                                templateUrl: "app/shared/helpText/helpTextModal.view.html",
                                controller: ["$scope", "$uibModalInstance", "notify", function ($scope, $modalInstance, nofity) {
                                    $http.get("odata/HelpTexts?$filter=Key eq '" + parent.key + "'")
                                        .then(function onSuccess(result) {
                                            if (result.data.value.length) {
                                                $scope.title = result.data.value[0].Title;
                                                $scope.description = $sce.trustAsHtml(result.data.value[0].Description);
                                            } else {
                                                $scope.title = parent.defaultTitle;
                                                $scope.description = "Ingen hjælpetekst defineret.";
                                            }
                                        })
                                }]
                            });
                        }
                    }]
            };
        }
    ]);
})(angular, app);
