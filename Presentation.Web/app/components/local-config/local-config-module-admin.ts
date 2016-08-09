module Kitos.LocalAdmin.ModuleAdmin {
    "use strict";

    export class ModuleAdminController {
        public orgAutosaveUrl: string;
        public typeName: string;
        public static $inject: string[] = ['$scope', '$http', 'notify', 'organization'];

        constructor(private $scope, private $http, private notify, public organization) {
            switch (organization.typeId) {
                case 1: this.typeName = 'Kommune'; break;
                case 2: this.typeName = 'Interessefællesskab'; break;
                case 3: this.typeName = 'Virksomhed'; break;
                case 4: this.typeName = 'Anden offentlig myndighed'; break;
            }

            this.orgAutosaveUrl = 'api/organization/' + organization.id;
        }
    }

    angular
        .module("app")
        .config([
            '$stateProvider', ($stateProvider: ng.ui.IStateProvider) => {
                $stateProvider.state('local-config.module-admin', {
                    url: '/org',
                    templateUrl: 'app/components/local-config/local-config-module-admin.html',
                    controller: ModuleAdminController,
                    controllerAs: 'vm',
                    resolve: {
                        organization: ['$http', 'userService', ($http: ng.IHttpService, userService) => {
                            return userService.getUser().then((user) => {
                                return $http.get<Kitos.API.Models.IApiWrapper<any>>('api/organization/' + user.currentOrganizationId).then((result) => {
                                    return result.data.response;
                                });
                            });
                        }]
                    }
                });
            }]);
}
