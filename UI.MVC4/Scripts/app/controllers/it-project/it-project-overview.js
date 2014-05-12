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
                }]
            }
        });
    }]);

    app.controller('project.EditOverviewCtrl',
    ['$scope', '$http', 'projects', 'projectRoles',
        function ($scope, $http, projects, projectRoles) {
            $scope.projects = projects;
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
        }]);
})(angular, app);
