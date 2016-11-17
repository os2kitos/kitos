module Kitos.GlobalAdmin.HelpTexts {
    "use strict";

    export class HelpTextsController {

        public static $inject: string[] = ['$rootScope', '$scope', '$http', 'notify', 'user', 'helpTexts'];
        public selectedHelpText;
        public tinymceOptions;

        constructor(private $rootScope, private $scope: ng.IScope, private $http, private notify, private user, private helpTexts) {
            $rootScope.page.title = 'Hjælpetekter';
            this.selectedHelpText = helpTexts[0];
            this.tinymceOptions = {
                plugins: 'link image code',
                toolbar: 'undo redo | bold italic | alignleft aligncenter alignright | code'
            };
        }

        private selectHelpText = function (helpText) {
            this.selectedHelpText = helpText;
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