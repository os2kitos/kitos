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
    ['$scope', '$http', 'notify', 'projectRoles', 'user',
        function ($scope, $http, notify, projectRoles, user) {
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

                    _.each(result.response, pushProject);

                }).error(function () {
                    notify.addErrorMessage("Kunne ikke hente projekter!");
                });
            }
            
            function pushProject(project) {
                // set current phase
                var phases = [project.phase1, project.phase2, project.phase3, project.phase4, project.phase5];
                project.currentPhase = _.find(phases, function (phase) {
                    return phase.id == project.currentPhaseId;
                });
                $scope.projects.push(project);
            }
        }]);
})(angular, app);
