module Kitos.Shared.Directives.HelpText {
    "use strict";
    
    export class FieldHelpTextController {
        static $inject: Array<string> = [
            "$scope",
            "$uibModal",
            "helpTextService",
            "userService"
        ];
        extraCssClass: string;
        isKeyDefined: boolean;
        private readonly key: string;

        constructor(
            $scope,
            private readonly $uibModal,
            private readonly helpTextService: Services.IHelpTextService,
            private readonly userService) {

            this.key = $scope.key;
            this.isKeyDefined = !!$scope.key;
            this.extraCssClass = $scope.extraCssClass;
        }

        showHelpTextModal = () =>
            Helpers.HelpTextModalHelper.openHelpTextModal(this.$uibModal,
                this.key,
                this.helpTextService,
                this.userService);
    }

    angular
        .module("app")
        .directive("fieldHelpText",
            [
                () => ({
                    templateUrl: "app/shared/helpText/field-help-text.view.html",
                    scope: {
                        key: "@",
                        extraCssClass: "@"
                    },
                    controller: FieldHelpTextController,
                    controllerAs: "fieldHelpTextVm"
                })
            ]);
}