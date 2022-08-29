((ng, app) => {
    "use strict";

    app.controller("fieldHelpTextController", ["$scope", "$uibModal", "helpTextService", "userService", function ($scope, $uibModal, helpTextService: Kitos.Services.IHelpTextService, userService) {
        const vm = this;

        vm.showHelpTextModal = () =>
            Kitos.Helpers.HelpTextModalHelper.openHelpTextModal($uibModal,
                $scope.key,
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