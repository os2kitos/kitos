(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.portfolio', {
            url: '/portfolio',
            templateUrl: 'partials/it-project/portfolio.html',
            controller: 'project.EditPortfolioCtrl',
            resolve: {
                projects: ['$http', 'userService', function($http, userService) {
                    return userService.getUser().then(function(user) {
                        return $http.get('api/itproject?orgId=' + user.currentOrganizationId).then(function(result) {
                            return result.data.response;
                        });
                    });
                }],
                projectRoles: ['$http', function ($http) {
                    return $http.get('api/itprojectrole').then(function (result) {
                        return result.data.response;
                    });
                }],
                orgUnits: ['$http', 'userService', function ($http, userService) {
                    return userService.getUser().then(function(user) {
                        return $http.get('api/organizationunit?userId=' + user.id).then(function(result) {
                            return result.data.response;
                        });
                    });
                }],
                user: ['userService', function (userService) {
                    return userService.getUser();
                }]
            }
        });
    }]);

    app.controller('project.EditPortfolioCtrl',
    ['$scope', '$http', '$sce', '$timeout', '$filter', 'projects', 'projectRoles', 'orgUnits', 'user',
        function ($scope, $http, $sce, $timeout, $filter, projects, projectRoles, orgUnits, user) {
            $scope.projects = projects;
            $scope.allProjects = projects;
            $scope.projectRoles = projectRoles;

            _.each(projects, function (project) {
                // fetch assigned roles for each project
                $http.get('api/itproject/' + project.id + '?rights').success(function (result) {
                    project.roles = result.response;
                });

                // set current phase
                var phases = [project.phase1, project.phase2, project.phase3, project.phase4, project.phase5];
                project.currentPhase = _.find(phases, function (phase) {
                    return phase.id == project.currentPhaseId;
                });
            });

            $scope.orgUnits = {};
            $scope.orgUnitTree = [];

            _.each(orgUnits, function (orgUnit) {
                visitOrgUnit(orgUnit, "");
            });

            checkForDefaultUnit();

            function filterProjects() {
                if ($scope.chosenOrgUnitId == 0) {
                    // 'alle' has been selected
                    $scope.projects = projects;
                } else if ($scope.chosenOrgUnitId == -1) {
                    // 'tværgående' has been selected
                    $scope.projects = $filter('filter')(projects, { isTransversal: true });
                } else {
                    $scope.projects = $filter('andChildren')(projects, 'responsibleOrgUnitId', $scope.orgUnitTree, $scope.chosenOrgUnitId);
                }
            }

            $scope.filterProjects = filterProjects;

            function visitOrgUnit(orgUnit) {

                $scope.orgUnits[orgUnit.id] = orgUnit;
                $scope.orgUnitTree.push(orgUnit);

                _.each(orgUnit.children, function (child) {
                    return visitOrgUnit(child);
                });
            }

            function checkForDefaultUnit() {
                if (!user.currentOrganizationUnitId) return;

                $scope.chosenOrgUnitId = user.currentOrganizationUnitId;
                filterProjects();
            }

            $scope.selectOrgUnitOptions = {
                dropdownAutoWidth: true,
                escapeMarkup: function (m) { return m; }
            };
        }]);
})(angular, app);
