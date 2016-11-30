(function (ng, app) {
    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
        $stateProvider.state('it-system.edit', {
            url: '/edit/{id:[0-9]+}',
            templateUrl: 'app/components/it-system/edit/it-system-edit.view.html',
            controller: 'system.EditCtrl',
            resolve: {
                itSystem: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itsystem/" + $stateParams.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                user: [
                    'userService', function (userService) {
                        return userService.getUser();
                    }
                ],
                hasWriteAccess: ['$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                    return $http.get("api/itsystem/" + $stateParams.id + "?hasWriteAccess=true&organizationId=" + user.currentOrganizationId)
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('system.EditCtrl',
    [
        '$rootScope', '$scope', 'itSystem',
        function ($rootScope, $scope, itSystem, user, hasWriteAccess) {
            $scope.hasWriteAccess = hasWriteAccess;
        }
    ]);
})(angular, app);
