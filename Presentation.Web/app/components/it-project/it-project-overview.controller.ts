﻿module Kitos.ItProject.Overview {
    'use strict';

    export interface IOverviewController {
        pagination: IPaginationSettings;
        csvUrl: string;
        projectRoles: any;
        projects: Array<any>;
        user: any;
        totalCount: number;
    }

    export class OverviewController implements IOverviewController {
        pagination: IPaginationSettings;
        csvUrl: string;
        projects: Array<any>;
        totalCount: number;

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
                    this.$http.get('api/itproject/' + iteratee.id + '?hasWriteAccess=true' + '&organizationId=' + this.user.currentOrganizationId)
                        .then(
                            (result: ng.IHttpPromiseCallbackArg<IApiResponse<any>>) => {
                                iteratee.canBeEdited = result.data.response;
                                deferred.resolve(iteratee);
                            },
                            result => {
                                iteratee.canBeEdited = false;
                                deferred.reject(result);
                            }
                        );
                }, 0);

                return deferred.promise;
            }));
        }

        private pushProject(project) {
            // due to https://github.com/angular/angular.js/blob/master/CHANGELOG.md#breaking-changes-8
            // we have to convert these values to strings
            project.priority = project.priority.toString();
            project.priorityPf = project.priorityPf.toString();

            this.projects.push(project);
        }

        private loadProjects() {
            // apparently not used
            // var deferred = this.$q.defer();

            var url = 'api/itProject?overview=true&orgId=' + this.user.currentOrganizationId;

            url += '&skip=' + this.pagination.skip;
            url += '&take=' + this.pagination.take;

            if (this.pagination.orderBy) {
                url += '&orderBy=' + this.pagination.orderBy;
                if (this.pagination.descending) {
                    url += '&descending=' + this.pagination.descending;
                }
            }

            if (this.pagination.search) {
                url += '&q=' + this.pagination.search;
            } else {
                url += '&q=';
            }

            this.projects = [];
            this.$http.get(url)
                .then(
                    (result: ng.IHttpPromiseCallbackArg<IApiResponse<any>>) => {
                        var headers = result.headers;
                        var paginationHeader = JSON.parse(headers('X-Pagination'));
                        this.totalCount = paginationHeader.TotalCount;

                        this.setCanEdit(result.data.response)
                            .then(canEditResult => angular.forEach(canEditResult, (project) => this.pushProject(project)));
                    },
                    () => this.notify.addErrorMessage('Kunne ikke hente projekter!')
                );
        }
    }

    angular
        .module('app')
        .config(['$stateProvider', $stateProvider => {
            $stateProvider.state('it-project.overview', {
                url: '/overview',
                templateUrl: 'app/components/it-project/it-project-overview.view.html',
                controller: OverviewController,
                controllerAs: 'projectOverviewVm',
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
