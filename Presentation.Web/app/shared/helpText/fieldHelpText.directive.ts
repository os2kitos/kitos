((ng, app) => {
    "use strict";

    app.directive("fieldHelpText", [
        () => ({
            templateUrl: "app/shared/helpText/fieldHelpText.view.html",
            scope: {
                key: "@",
                extraCssClass: "@"
            },
            controller: [
                "$scope", "$uibModal", "helpTextService", "userService", ($scope, $uibModal, helpTextService: Kitos.Services.IHelpTextService, userService) => {
                    $scope.showHelpTextModal = () =>
                        Kitos.Helpers.HelpTextModalHelper.openHelpTextModal($uibModal,
                            $scope,
                            helpTextService,
                            userService);
                }]
        })
    ]);
})(angular, app);