module Kitos.GlobalAdmin.HelpTexts {
    "use strict";

    export class HelpTextsEditController {

        static $inject: string[] = ["$rootScope", "notify", "helpText", "_", "$state", "helpTextService"];
        tinymceOptions;
        selectedHelpText: Models.IHelpText;

        constructor(
            $rootScope,
            private readonly notify,
            public helpText,
            private readonly _,
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

        private save() {
            var msg = this.notify.addInfoMessage("Gemmer...", false);
            this.helpTextService.updateHelpText(this.helpText.Id, this.helpText.Key, this.helpText.Title, this.helpText.Description)
                .then(function onSuccess(result) {
                    msg.toSuccessMessage("Feltet er gemt.");
                }, function onError(result) {
                    if (result.status === 409) {
                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres da værdien den allerede findes i KITOS!");
                    } else {
                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                    }
                });
        }

        private delete() {
            if (!confirm("Er du sikker på at du vil slette hjælpeteksten?")) {
                return;
            }
            var msg = this.notify.addInfoMessage("Sletter...", false);
            var parent = this;
            this.helpTextService.deleteHelpText(this.helpText.Id,this.helpText.Key)
                .then(function onSuccess(result) {
                    msg.toSuccessMessage("Hjælpeteksten er slettet.");
                    parent.$state.go("global-admin.help-texts");
                }, function onError(result) {
                    if (result.status === 409) {
                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres da værdien den allerede findes i KITOS!");
                    } else {
                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                    }
                });
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