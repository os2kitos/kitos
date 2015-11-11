(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.description', {
            url: '/description',
            templateUrl: 'app/components/it-project/tabs/it-project-tab-description.html',
            controller: 'project.EditDescriptionCtrl',
            resolve: {
                // re-resolve data from parent cause changes here wont cascade to it
                project: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itproject/" + $stateParams.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('project.EditDescriptionCtrl',
    ['$scope', 'project',
        function ($scope, project) {
            $scope.itProjectId = project.id;
            $scope.description = project.description;
        }]);
})(angular, app);
