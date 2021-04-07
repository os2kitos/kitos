module Kitos.GlobalAdmin.HelpTexts {
    "use strict";

    export class HelpTextsEditController {

        public static $inject: string[] = ['$rootScope', '$scope', '$http', 'notify', 'user', 'helpText', '$uibModal', '_', '$state'];
        public tinymceOptions;
        public selectedHelpText: Models.IHelpText;

        constructor(private $rootScope, private $scope: ng.IScope, private $http, private notify, private user, public helpText, private $uibModal, private _, private $state) {
            $rootScope.page.title = 'Hjælpetekter';
            this.tinymceOptions = {
                plugins: 'link image code',
                skin: 'lightgray',
                theme: 'modern',
                convert_urls: false,
                height: '400px'
            };
        }

        private save() {
            var payload = {
                Title: this.helpText.Title,
                Description: this.helpText.Description
            };
            var msg = this.notify.addInfoMessage("Gemmer...", false);
            this.$http({ method: 'PATCH', url: "odata/HelpTexts(" + this.helpText.Id + ")", data: payload, ignoreLoadingBar: true })
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
            if (!confirm('Er du sikker på at du vil slette hjælpeteksten?')) {
                return;
            }
            var msg = this.notify.addInfoMessage("Sletter...", false);
            var parent = this;
            this.$http({ method: 'DELETE', url: "odata/HelpTexts(" + this.helpText.Id + ")" })
                .then(function onSuccess(result) {
                    msg.toSuccessMessage("Hjælpeteksten er slettet.");
                    parent.$state.go('global-admin.help-texts');
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
            '$stateProvider', ($stateProvider) => {
                $stateProvider.state('global-admin.help-texts-edit', {
                    url: "/help-texts/edit/{id:[0-9]+}",
                    templateUrl: 'app/components/global-admin/global-admin-help-texts-edit.view.html',
                    controller: HelpTextsEditController,
                    controllerAs: 'helpTextsEditCtrl',
                    authRoles: ['GlobalAdmin'],
                    resolve: {
                        user: [
                            'userService', (userService) => {
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