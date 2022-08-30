((ng, app) => {
    "use strict";

    app.controller("helpTextController", ["$scope", "$uibModal", "helpTextService", "userService", function ($scope, $uibModal, helpTextService: Kitos.Services.IHelpTextService, userService) {
        const vm = this;
        const key = $scope.key;
        vm.noButtonLayout = $scope.noButtonLayout;

        vm.showHelpTextModal = () =>
            Kitos.Helpers.HelpTextModalHelper.openHelpTextModal($uibModal,
                key,
                helpTextService,
                userService);
        return vm;
    }]);

    app.directive("helpText", [
            () => ({
                templateUrl: "app/shared/helpText/helpText.view.html",
                scope: {
                    key: "@",
                    defaultTitle: "@",
                    noButtonLayout: "@"
                },
            controller: "helpTextController",
                controllerAs: "helpTextVm"
            })
        ]);
})(angular, app);
