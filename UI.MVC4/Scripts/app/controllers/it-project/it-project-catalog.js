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
        ['$scope', '$http', '$state', 'user', function ($scope, $http, $state, user) {
            $scope.create = function () {
                var orgUnitId = user.defaultOrganizationUnitId;
                var payload = {
                    itProjectTypeId: 1,
                    ItProjectCategoryId: 1,
                    responsibleOrgUnitId: orgUnitId,
                    organizationId: user.currentOrganizationId,
                };
                $http.post('api/itproject', payload)
                    .success(function (result) {
                        var projectId = result.response.id;
                        if (orgUnitId) {
                            // add users default org unit to the new project
                            $http.post('api/itproject/' + projectId + '?organizationunit=' + orgUnitId);
                        }
                        $state.go('it-project.edit', { id: projectId });
                    })
                    .error(function() {
                        
                    });
            };
        }]
    );
})(angular, app);