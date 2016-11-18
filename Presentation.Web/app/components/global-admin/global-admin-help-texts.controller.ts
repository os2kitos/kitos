module Kitos.GlobalAdmin.HelpTexts {
    "use strict";

    export class HelpTextsController {

        public static $inject: string[] = ['$rootScope', '$scope', '$http', 'notify', 'user', 'helpTexts', '$uibModal', '_'];
        public selectedHelpText;
        public tinymceOptions;

        constructor(private $rootScope, private $scope: ng.IScope, private $http, private notify, private user, private helpTexts, private $uibModal, private _) {
            $rootScope.page.title = 'Hjælpetekter';
            this.selectDefaultHelpText();
            this.tinymceOptions = {
                plugins: 'link image code',
                toolbar: 'undo redo | bold italic | alignleft aligncenter alignright | code'
            };
        }

        private selectHelpText = function (helpText) {
            this.selectedHelpText = helpText;
        }

        private selectDefaultHelpText() {
            console.log(this.helpTexts.length);
            if (this.helpTexts.length > 0) {
                this.selectedHelpText = this.helpTexts[0];
            } else {
                this.selectedHelpText = null;
            }
        }

        private save() {
            var msg = this.notify.addInfoMessage("Gemmer...", false);
            this.$http({ method: 'PATCH', url: "odata/HelpTexts(" + this.selectedHelpText.Id + ")", data: this.selectedHelpText, ignoreLoadingBar: true })
                .success(function () {
                    msg.toSuccessMessage("Feltet er gemt.");
                })
                .error(function (result, status) {
                    if (status === 409) {
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
            this.$http({ method: 'DELETE', url: "odata/HelpTexts(" + this.selectedHelpText.Id + ")" })
                .success(function () {
                    msg.toSuccessMessage("Hjælpeteksten er slettet.");
                    _.remove(parent.helpTexts, function (obj) {
                        return obj === parent.selectedHelpText;
                    });

                    parent.selectDefaultHelpText();
                })
                .error(function (result, status) {
                    if (status === 409) {
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
                $stateProvider.state('global-admin.help-texts', {
                    url: '/help-texts',
                    templateUrl: 'app/components/global-admin/global-admin-help-texts.view.html',
                    controller: HelpTextsController,
                    controllerAs: 'helpTextsCtrl',
                    authRoles: ['GlobalAdmin'],
                    resolve: {
                        user: [
                            'userService', (userService) => {
                                return userService.getUser();
                            }
                        ],
                        helpTexts: [
                            "$http", $http => $http.get("/odata/HelpTexts").then(result => result.data.value)
                        ]
                    }
                });
            }
    ]);
}