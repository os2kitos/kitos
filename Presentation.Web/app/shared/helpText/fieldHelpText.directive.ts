((ng, app) => {
    "use strict";

    app.controller("fieldHelpTextController", ["$scope", "$uibModal", "helpTextService", "userService", function ($scope, $uibModal, helpTextService: Kitos.Services.IHelpTextService, userService) {
        const vm = this;
        const key = $scope.key;
        vm.extraCssClass = $scope.extraCssClass;

        vm.showHelpTextModal = () =>
            Kitos.Helpers.HelpTextModalHelper.openHelpTextModal($uibModal,
                key,
                helpTextService,
                userService);
        return vm;
    }]);

    app.directive("fieldHelpText", [
        () => ({
            templateUrl: "app/shared/helpText/fieldHelpText.view.html",
            scope: {
                key: "@",
                extraCssClass: "@"
            },
            controller: "fieldHelpTextController",
            controllerAs: "helpTextVm"
        })
    ]);
})(angular, app);