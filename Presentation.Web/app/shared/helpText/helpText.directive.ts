((ng, app) => {
    "use strict";

    app.directive("helpText", [
        () => ({
            templateUrl: "app/shared/helpText/helpText.view.html",
            scope: {
                key: "@",
                defaultTitle: "@",
                noButtonLayout: "@"
            },
            controller: [
                "$scope", "$uibModal", "helpTextService", ($scope, $uibModal, helpTextService : Kitos.Services.IHelpTextService) => {
                    var parent = $scope;

                    $scope.showHelpTextModal = () => {
                        $uibModal.open({
                            windowClass: "modal fade in",
                            templateUrl: "app/shared/helpText/helpTextModal.view.html",
                            controller: ["$scope", ($scope) => {

                                helpTextService.loadHelpText(parent.key)
                                    .then(helpText => {
                                        if (helpText != null) {
                                            $scope.title = helpText.title;
                                            $scope.description = helpText.htmlText;
                                        } else {
                                            $scope.title = parent.defaultTitle;
                                            $scope.description = "Ingen hjælpetekst defineret.";
                                        }
                                    });
                            }]
                        });
                    }
                }]
        })
    ]);
})(angular, app);
