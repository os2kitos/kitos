module Kitos.Shared.Directives.HelpText {
    "use strict";

    export class HelpTextController {
        static $inject: Array<string> = [
            "$scope",
            "$uibModal",
            "helpTextService",
            "userService"
        ];

        noButtonLayout: string;

        constructor(
            private readonly $scope,
            private readonly $uibModal,
            private readonly helpTextService: Services.IHelpTextService,
            private readonly userService) {
            
            this.noButtonLayout = $scope.noButtonLayout;
        }

        showHelpTextModal = () =>
            Helpers.HelpTextModalHelper.openHelpTextModal(this.$uibModal,
                this.$scope.key,
                this.helpTextService,
                this.userService);
    }

    angular
        .module("app")
        .directive("helpText",
            () => ({
                templateUrl: "app/shared/helpText/helpText.view.html",
                scope: {
                    key: "@",
                    defaultTitle: "@",
                    noButtonLayout: "@"
                },
                controller: HelpTextController,
                controllerAs: "helpTextVm"
            })
        );
}