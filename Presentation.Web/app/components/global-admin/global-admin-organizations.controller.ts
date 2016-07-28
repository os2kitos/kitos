module Kitos.GlobalAdmin.Organization {
    "use strict";

    export class OrganizationController {
        public pagination;
        public totalCount: number;
        public organizations;
        public static $inject: string[] = ['$rootScope', '$scope', '$http', 'notify', 'user'];

        constructor(private $rootScope, private $scope: ng.IScope, private $http, private notify, private user) {
            $rootScope.page.title = 'Organisationer';

            this.pagination = {
                search: '',
                skip: 0,
                take: 100
            };

            $scope.$watchCollection(() => this.pagination, () => {
                this.loadOrganizations();
            });
        }

        private loadOrganizations() {
            var url = 'api/organization/';
            url += '?skip=' + this.pagination.skip + "&take=" + this.pagination.take;

            if (this.pagination.orderBy) {
                url += '&orderBy=' + this.pagination.orderBy;
                if (this.pagination.descending) url += '&descending=' + this.pagination.descending;
            }

            if (this.pagination.search) url += '&q=' + this.pagination.search;
            else url += "&q=";

            this.$http.get(url).success((result, status, headers) => {
                var paginationHeader = JSON.parse(headers('X-Pagination'));
                this.totalCount = paginationHeader.TotalCount;
                this.organizations = result.response;
            }).error(() => {
                this.notify.addErrorMessage("Kunne ikke hente organisationer!");
            });
        }

        public delete(orgId) {
            this.$http.delete('api/organization/' + orgId + '?organizationId=' + this.user.currentOrganizationId)
                .success(() => {
                    this.notify.addSuccessMessage("Organisationen er blevet slettet!");
                    this.loadOrganizations();
                })
                .error(() => {
                    this.notify.addErrorMessage("Kunne ikke slette organisationen!");
                });
        }
    }

    angular
        .module("app")
        .config([
            '$stateProvider', ($stateProvider) => {
                $stateProvider.state('global-admin.organizations', {
                    url: '/organisations',
                    templateUrl: 'app/components/global-admin/global-admin-organizations.view.html',
                    controller: OrganizationController,
                    controllerAs: 'orgCtrl',
                    authRoles: ['GlobalAdmin'],
                    resolve: {
                        user: [
                            'userService', (userService) => {
                                return userService.getUser();
                            }
                        ]
                    }
                });
            }
        ]);
}
