(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.strategy', {
            url: '/strategy',
            templateUrl: 'partials/it-project/tab-strategy.html',
            controller: 'project.EditStrategyCtrl',
            resolve: {
                jointMunicipalProjects: ['$http', 'project', 'projectTypes', function ($http, project, projectTypes) {
                    var category = _.find(projectTypes, function(cat) {
                        return cat.name == 'Fælleskommunal'; // TODO hardcoded literal... find better solution!
                    });
                    var catId = category.id;
                    var orgId = project.organizationId;
                    return $http.get('api/itproject/?orgId=' + orgId + '&catId=' + catId).then(function(result) {
                        return result.data.response;
                    });
                }],
                commonPublicProjects: ['$http', 'project', 'projectTypes', function ($http, project, projectTypes) {
                    var category = _.find(projectTypes, function (cat) {
                        return cat.name == 'Fællesoffentlig'; // TODO hardcoded literal... find better solution!
                    });
                    var catId = category.id;
                    var orgId = project.organizationId;
                    return $http.get('api/itproject/?orgId=' + orgId + '&catId=' + catId).then(function (result) {
                        return result.data.response;
                    });
                }]
            }
        });
    }]);

    app.controller('project.EditStrategyCtrl',
    ['$scope', 'project', 'jointMunicipalProjects', 'commonPublicProjects',
        function ($scope, project, jointMunicipalProjects, commonPublicProjects) {
            $scope.isStrategy = project.isStrategy;
            $scope.jointMunicipalProjectId = project.jointMunicipalProjectId;
            $scope.jointMunicipalProjects = jointMunicipalProjects;
            $scope.commonPublicProjectId = project.commonPublicProjectId;
            $scope.commonPublicProjects = commonPublicProjects;

            $scope.autosaveUrl = 'api/itproject/' + project.id;
        }]);
})(angular, app);
