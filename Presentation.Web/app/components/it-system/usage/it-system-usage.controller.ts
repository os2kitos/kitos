(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage', {
            url: '/usage/{id:[0-9]+}',
            templateUrl: 'app/components/it-system/usage/it-system-usage.view.html',
            controller: 'system.UsageCtrl',
            resolve: {
                user: [
                    'userService', function (userService) {
                        return userService.getUser();
                    }
                ],
                hasWriteAccess: [
                    '$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                        return $http.get('api/itSystemUsage/' + $stateParams.id + "?hasWriteAccess=true&organizationId=" + user.currentOrganizationId)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }
                ],
                itSystemUsage: [
                    '$http', '$stateParams', function ($http, $stateParams) {
                        return $http.get('api/itSystemUsage/' + $stateParams.id)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }
                ],
            }
        });
    }]);

    app.controller('system.UsageCtrl', ['$rootScope', '$scope', 'itSystemUsage', 'user', 'hasWriteAccess',
        function ($rootScope, $scope, itSystemUsage, user, hasWriteAccess) {
            $scope.hasWriteAccess = hasWriteAccess;
            $scope.usage = itSystemUsage;
            $scope.allowClearOption = {
                allowClear: true
            };
        }
    ]);
})(angular, app);
