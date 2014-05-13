(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.overview', {
            url: '/overview',
            templateUrl: 'partials/it-project/overview.html',
            controller: 'project.EditOverviewCtrl',
            resolve: {
                projects: ['$http', function($http) {
                    return $http.get('api/itproject').then(function(result) {
                        return result.data.response;
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

    app.controller('project.EditOverviewCtrl',
    ['$scope', '$http', '$sce', '$timeout', '$filter', 'projects', 'projectRoles', 'orgUnits', 'user',
        function ($scope, $http, $sce, $timeout, $filter, projects, projectRoles, orgUnits, user) {
            $scope.projects = projects;
            $scope.allProjects = projects;
            $scope.projectRoles = projectRoles;
            
            _.each(projects, function(project) {
                // fetch assigned roles for each project
                $http.get('api/itprojectright/' + project.id).success(function (result) {
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

            _.each(orgUnits, function(orgUnit) {
                visitOrgUnit(orgUnit, "");
                hasWriteAccess(orgUnit, false);
            });

            $scope.$watch('chosenOrgUnitId', function (newValue) {
                // this is so hacky... everytime this executes a kitten dies
                if (newValue == 't') {
                    // 'tværgående' has been selected
                    $scope.projects = $filter('filter')(projects, { isTransversal: true });
                } else {
                    $scope.projects = $filter('andChildren')(projects, 'responsibleOrgUnitId', $scope.orgUnitTree, newValue);
                }
            });

            function visitOrgUnit(orgUnit, indentation) {

                orgUnit.indentation = $sce.trustAsHtml(indentation);

                checkForDefaultUnit(orgUnit);

                $scope.orgUnits[orgUnit.id] = orgUnit;
                $scope.orgUnitTree.push(orgUnit);

                _.each(orgUnit.children, function (child) {
                    return visitOrgUnit(child, indentation + "&nbsp;&nbsp;&nbsp;");
                });
            }
            
            function checkForDefaultUnit(unit) {
                if (!user.defaultOrganizationUnitId) return;

                if (!unit || unit.id !== user.defaultOrganizationUnitId) return;

                $timeout(function () {
                    $scope.chosenOrgUnitId = unit.id;
                });
            }
            
            function hasWriteAccess(orgUnit, inherit) {
                if (inherit) {
                    orgUnit.hasWriteAccess = true;

                    _.each(orgUnit.children, function (child) {
                        hasWriteAccess(child, true);
                    });
                } else {
                    $http.get('api/organizationRight?hasWriteAccess&oId=' + orgUnit.id + '&uId=' + user.id).success(function (result) {
                        orgUnit.hasWriteAccess = result.response;

                        _.each(orgUnit.children, function (child) {
                            hasWriteAccess(child, result.response);
                        });
                    });
                }
            }
            
            $scope.selectOrgUnitOptions = {
                dropdownAutoWidth : true,
                escapeMarkup: function (m) { return m; }
            };
        }]);
})(angular, app);
