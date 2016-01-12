(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.strategy', {
            url: '/strategy',
            templateUrl: 'app/components/it-project/tabs/it-project-tab-strategy.html',
            controller: 'project.EditStrategyCtrl',
            resolve: {
                // re-resolve data from parent cause changes here wont cascade to it
                project: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itproject/" + $stateParams.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                jointMunicipalProjects: ['$http', 'project', 'projectTypes', function ($http, project, projectTypes) {
                    var type: { id } = _.find(projectTypes, function (t: { id; name; }) {
                        return t.name == 'Fælleskommunal'; // TODO hardcoded literal... find better solution!
                    });
                    var typeId = type.id;
                    var orgId = project.organizationId;
                    return $http.get('api/itproject/?orgId=' + orgId + '&typeId=' + typeId).then(function (result) {
                        return result.data.response;
                    });
                }],
                commonPublicProjects: ['$http', 'project', 'projectTypes', function ($http, project, projectTypes) {
                    var type = _.find(projectTypes, function (t: { id; name; }) {
                        return t.name == 'Fællesoffentlig'; // TODO hardcoded literal... find better solution!
                    });
                    var typeId = type.id;
                    var orgId = project.organizationId;
                    return $http.get('api/itproject/?orgId=' + orgId + '&typeId=' + typeId).then(function (result) {
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
