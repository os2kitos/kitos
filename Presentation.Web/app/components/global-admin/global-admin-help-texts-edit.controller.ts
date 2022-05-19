module Kitos.GlobalAdmin.HelpTexts {
    "use strict";

    export class HelpTextsEditController {

        static $inject: string[] = ["$rootScope", "helpText", "$state", "helpTextService"];
        tinymceOptions;
        selectedHelpText: Models.IHelpText;

        constructor(
            $rootScope,
            public helpText,
            private readonly $state,
            private readonly helpTextService: Services.IHelpTextService) {
            $rootScope.page.title = "Hjælpetekter";
            this.tinymceOptions = {
                plugins: "link image code",
                skin: "lightgray",
                theme: "modern",
                convert_urls: false,
                height: "400px"
            };
        }

        save() {
            this.helpTextService.updateHelpText(
                this.helpText.Id,
                this.helpText.Key,
                this.helpText.Title,
                this.helpText.Description
            );
        }

        delete() {
            if (!confirm("Er du sikker på at du vil slette hjælpeteksten?")) {
                return;
            }
            var parent = this;
            this.helpTextService
                .deleteHelpText(this.helpText.Id, this.helpText.Key)
                .then(_ => parent.$state.go("global-admin.help-texts"));
        }
    }

    angular
        .module("app")
        .config([
            "$stateProvider", ($stateProvider) => {
                $stateProvider.state("global-admin.help-texts-edit", {
                    url: "/help-texts/edit/{id:[0-9]+}",
                    templateUrl: "app/components/global-admin/global-admin-help-texts-edit.view.html",
                    controller: HelpTextsEditController,
                    controllerAs: "helpTextsEditCtrl",
                    authRoles: ["GlobalAdmin"],
                    resolve: {
                        user: [
                            "userService", (userService) => {
                                return userService.getUser();
                            }
                        ],
                        helpText: [
                            "$http", "$stateParams", ($http, $stateParams) => $http.get("/odata/HelpTexts(" + $stateParams.id + ")").then(result => result.data)
                        ]
                    }
                });
            }
        ]);
}