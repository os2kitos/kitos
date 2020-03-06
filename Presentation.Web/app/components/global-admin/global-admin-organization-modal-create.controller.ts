module Kitos.GlobalAdmin.Organization {
    "use strict";

    export class CreateOrganizationController {
        public title: string;
        public org;
        public buttonDisabled: boolean;

        public static $inject: string[] = ['$rootScope', '$scope', '$http', 'notify'];

        constructor(private $rootScope, private $scope, private $http, private notify) {
            var orgViewModel = new Models.ViewModel.GlobalAdmin.Organization.OrganizationModalViewModel();
            orgViewModel.configureAsNewOrganizationDialog();
            $rootScope.page.title = orgViewModel.title;
            $scope.title = orgViewModel.title;
            $scope.org = orgViewModel.data;
            this.buttonDisabled = false;
        }

        public dismiss() {
            this.$scope.$dismiss();
        }

        public submit() {
            this.buttonDisabled = true;
            var payload = this.$scope.org;
            this.$http.post('api/organization', payload)
                .success((result) => {
                    this.notify.addSuccessMessage(`Organisationen ${result.response.name} er blevet oprettet!`);
                    this.$scope.$close(true);
                })
                .error((result) => {
                    this.notify.addErrorMessage(`Organisationen ${this.org.name} kunne ikke oprettes!`);
                });
        }
    }

    angular
        .module("app")
        .config([
            '$stateProvider', ($stateProvider) => {
                $stateProvider.state('global-admin.organizations.create', {
                    url: '/create',
                    authRoles: ['GlobalAdmin'],
                    onEnter: ['$state', '$stateParams', '$uibModal',
                        ($state: ng.ui.IStateService, $stateParams: ng.ui.IStateParamsService, $modal: ng.ui.bootstrap.IModalService) => {
                            $modal.open({
                                size: 'lg',
                                templateUrl: 'app/components/global-admin/global-admin-organization-modal.view.html',
                                // fade in instead of slide from top, fixes strange cursor placement in IE
                                // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                                windowClass: 'modal fade in',
                                controller: CreateOrganizationController,
                                controllerAs: "ctrl"
                            }).result.then(() => {
                                // OK
                                // GOTO parent state and reload
                                $state.go('^', null, { reload: true });
                            }, () => {
                                // Cancel
                                // GOTO parent state
                                $state.go('^');
                            });
                        }
                    ]
                });
            }
        ]);
}
