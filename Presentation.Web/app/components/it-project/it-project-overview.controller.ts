module Kitos.ItProject.Overview {
    'use strict';

    interface IPaginationSettings {
        search: string;
        skip: number;
        take: number;
        orderBy: string;
        descending?: boolean;
    }

    export class OverviewController {

        public pagination: IPaginationSettings
        public csvUrl: string;
        public projects: Array<any>
        public totalCount: number;

        static $inject: Array<string> = [
            '$scope',
            '$http',
            'notify',
            'projectRoles',
            'user',
            '$q'
        ];

        constructor(
            private $scope: ng.IScope,
            private $http: ng.IHttpService,
            private notify,
            public projectRoles,
            public user,
            private $q) {

            this.pagination = {
                search: '',
                skip: 0,
                take: 25,
                orderBy: 'Name'
            };

            this.csvUrl = 'api/itProject?csv&orgId=' + this.user.currentOrganizationId;

            this.projects = [];

            this.$scope.$watchCollection(() => this.pagination, () => this.loadProjects());
        }

        private setCanEdit(projectCollection) {
            return this.$q.all(_.map(projectCollection, (iteratee: { id; canBeEdited; }) => {
                var deferred = this.$q.defer();

                setTimeout(() => {
                    this.$http.get("api/itProject/" + iteratee.id + "?hasWriteAccess" + '&organizationId=' + this.user.currentOrganizationId)
                        .then(
                        (result: ng.IHttpPromiseCallbackArg<IApiResponse<any>>) => {
                            iteratee.canBeEdited = result.data.response;
                            deferred.resolve(iteratee);
                        }, result => {
                            iteratee.canBeEdited = false;
                            deferred.reject(result);
                        });
                }, 0);

                return deferred.promise;
            }));
        }

        private pushProject(project) {
            // Due to https://github.com/angular/angular.js/blob/master/CHANGELOG.md#breaking-changes-8
            // we have to convert these values to strings
            project.priority = project.priority.toString();
            project.priorityPf = project.priorityPf.toString();

            this.projects.push(project);
        }

        private loadProjects() {
            var deferred = this.$q.defer();

            var url = 'api/itProject?overview&orgId=' + this.user.currentOrganizationId;

            url += '&skip=' + this.pagination.skip;
            url += '&take=' + this.pagination.take;

            if (this.pagination.orderBy) {
                url += '&orderBy=' + this.pagination.orderBy;
                if (this.pagination.descending) url += '&descending=' + this.pagination.descending;
            }

            if (this.pagination.search) url += '&q=' + this.pagination.search;
            else url += "&q=";

            this.projects = [];
            this.$http.get(url)
                .then((result: ng.IHttpPromiseCallbackArg<IApiResponse<any>>) => {
                    var headers = result.headers;
                    var paginationHeader = JSON.parse(headers('X-Pagination'));
                    this.totalCount = paginationHeader.TotalCount;

                    this.setCanEdit(result.data.response)
                        .then(canEditResult => angular.forEach(canEditResult, (project) => this.pushProject(project)));
                },
                () => this.notify.addErrorMessage("Kunne ikke hente projekter!"));
        }
    }

    angular
        .module('app')
        .controller('project.EditOverviewCtrl', OverviewController)
        .config(['$stateProvider', $stateProvider => {
            $stateProvider.state('it-project.overview', {
                url: '/overview',
                templateUrl: 'app/components/it-project/it-project-overview.html',
                controller: OverviewController,
                controllerAs: 'vm',
                resolve: {
                    projectRoles: [
                        '$http', $http => $http.get('api/itprojectrole').then(result => result.data.response)
                    ],
                    user: [
                        'userService', userService => userService.getUser()
                    ]
                }
            });
        }]);
}
