(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.overview', {
            url: '/overview',
            templateUrl: 'partials/it-project/overview.html',
            controller: 'project.EditOverviewCtrl',
            resolve: {
                projectRoles: ['$http', function ($http) {
                    return $http.get('api/itprojectrole').then(function (result) {
                        return result.data.response;
                    });
                }],
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            }
        });
    }]);

    app.controller('project.EditOverviewCtrl',
    ['$scope', '$http', 'notify', 'projectRoles', 'user', '$q',
        function ($scope, $http, notify, projectRoles, user, $q) {
            $scope.pagination = {
                search: '',
                skip: 0,
                take: 25,
                orderBy: 'Name'
            };

            $scope.csvUrl = 'api/itProject?csv&orgId=' + user.currentOrganizationId;

            $scope.projects = [];
            $scope.projectRoles = projectRoles;
            
            $scope.$watchCollection('pagination', function (newVal, oldVal) {
                loadProjects();
            });

            function loadProjects() {
                var deferred = $q.defer();

                var url = 'api/itProject?overview&orgId=' + user.currentOrganizationId;

                url += '&skip=' + $scope.pagination.skip;
                url += '&take=' + $scope.pagination.take;

                if ($scope.pagination.orderBy) {
                    url += '&orderBy=' + $scope.pagination.orderBy;
                    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                }

                if ($scope.pagination.search) url += '&q=' + $scope.pagination.search;
                else url += "&q=";

                $scope.projects = [];
                $http.get(url).success(function (result, status, headers) {

                    var paginationHeader = JSON.parse(headers('X-Pagination'));
                    $scope.totalCount = paginationHeader.TotalCount;

                    setCanEdit(result.response).then(function(canEditResult) {
                        _.each(canEditResult, pushProject);
                    });

                }).error(function () {
                    notify.addErrorMessage("Kunne ikke hente projekter!");
                });
            }

            function setCanEdit(projectCollection) {
                return $q.all(_.map(projectCollection, function(iteratee) {
                    var deferred = $q.defer();

                    setTimeout(function() {
                        $http.get("api/itProject/" + iteratee.id + "?hasWriteAccess" + '&organizationId=' + user.currentOrganizationId)
                            .success(function(result) {
                                iteratee.canBeEdited = result.response;
                                deferred.resolve(iteratee);
                            })
                            .error(function(result) {
                                iteratee.canBeEdited = false;
                                deferred.reject(result);
                                }
                            );
                    }, 0);

                    return deferred.promise;
                }));
            }
            
            function pushProject(project) {
                $scope.projects.push(project);
            }
        }]);
})(angular, app);
