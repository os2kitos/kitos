(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('edit-it-project.itsys', {
            url: '/itsys',
            templateUrl: 'partials/it-project/tab-itsys.html',
            controller: 'project.EditItsysCtrl',
            resolve: {
                itSystems: ['$http', 'itProject', function ($http, itProject) {
                    return $http.get('api/itsystemusage/?orgId=' + itProject.organizationId)
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('project.EditItsysCtrl',
    ['$scope', 'itProject',
        function ($scope, itSystems) {
            $scope.itSystems = itSystems;

        }]);
})(angular, app);
