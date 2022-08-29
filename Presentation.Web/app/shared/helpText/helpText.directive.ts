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
                    "$scope", "$uibModal", "helpTextService", "userService", ($scope, $uibModal, helpTextService: Kitos.Services.IHelpTextService, userService) => {
                        $scope.showHelpTextModal = () =>
                            Kitos.Helpers.HelpTextModalHelper.openHelpTextModal($uibModal,
                                $scope.key,
                                helpTextService,
                                userService);
                    }]
            })
        ]);
})(angular, app);
