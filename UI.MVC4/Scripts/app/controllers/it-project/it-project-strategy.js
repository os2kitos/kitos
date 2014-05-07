(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('edit-it-project.strategy', {
            url: '/strategy',
            templateUrl: 'partials/it-project/tab-strategy.html',
            controller: 'project.EditStrategyCtrl',
            resolve: {
                jointMunicipalProjects: ['$http', 'itProject', 'itProjectCategories', function ($http, itProject, itProjectCategories) {
                    var category = _.find(itProjectCategories, function(cat) {
                        return cat.name == 'Fælleskommunal'; // TODO hardcoded literal... find better solution!
                    });
                    var catId = category.id;
                    var orgId = itProject.organizationId;
                    return $http.get('api/itproject/?orgId=' + orgId + '&catId=' + catId).then(function(result) {
                        return result.data.response;
                    });
                }],
                commonPublicProjects: ['$http', 'itProject', 'itProjectCategories', function ($http, itProject, itProjectCategories) {
                    var category = _.find(itProjectCategories, function (cat) {
                        return cat.name == 'Fællesoffentlig'; // TODO hardcoded literal... find better solution!
                    });
                    var catId = category.id;
                    var orgId = itProject.organizationId;
                    return $http.get('api/itproject/?orgId=' + orgId + '&catId=' + catId).then(function (result) {
                        return result.data.response;
                    });
                }]
            }
        });
    }]);

    app.controller('project.EditStrategyCtrl',
    ['$scope', 'itProject', 'jointMunicipalProjects', 'commonPublicProjects',
        function ($scope, itProject, jointMunicipalProjects, commonPublicProjects) {
            $scope.isStrategy = itProject.isStrategy;
            $scope.jointMunicipalProjectId = itProject.jointMunicipalProjectId;
            $scope.jointMunicipalProjects = jointMunicipalProjects;
            $scope.commonPublicProjectId = itProject.commonPublicProjectId;
            $scope.commonPublicProjects = commonPublicProjects;

            $scope.autosaveUrl = 'api/itproject/' + itProject.id;
        }]);
})(angular, app);
