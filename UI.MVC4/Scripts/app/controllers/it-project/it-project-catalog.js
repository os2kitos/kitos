(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.catalog', {
            url: '/catalog',
            templateUrl: 'partials/it-project/catalog.html',
            controller: 'project.CatalogCtrl',
            resolve: {
                user: ['userService', function(userService) {
                    return userService.getUser();
                }]
            }
        });
    }]);

    app.controller('project.CatalogCtrl',
        ['$scope', '$http', 'user', function ($scope, $http, user) {
            $scope.create = function () {
                var orgUnitId = user.defaultOrganizationUnitId;
                var payload = {
                    responsibleOrgUnitId: orgUnitId
                };
                $http.post('api/itproject', payload)
                    .sucess(function (result) {
                        var projectId = result.id;
                        // add users default org unit to the new project
                        $http.post('api/itproject/' + projectId + '?organizationunit=' + orgUnitId);
                    })
                    .error(function() {
                        
                    });
            };
        }]
    );
})(angular, app);