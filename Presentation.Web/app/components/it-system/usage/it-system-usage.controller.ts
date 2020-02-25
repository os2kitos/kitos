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
                userAccessRights: ["authorizationServiceFactory", "$stateParams",
                    (authorizationServiceFactory: Kitos.Services.Authorization.IAuthorizationServiceFactory, $stateParams) =>
                    authorizationServiceFactory
                    .createSystemUsageAuthorization()
                    .getAuthorizationForItem($stateParams.id)
                ],
                hasWriteAccess: [
                    "userAccessRights", (userAccessRights: Kitos.Models.Api.Authorization.EntityAccessRightsDTO) => userAccessRights.canEdit
                ],
                itSystemUsage: [
                    '$http', '$stateParams', function ($http, $stateParams) {
                        return $http.get('api/itSystemUsage/' + $stateParams.id)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }
                ]
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


            if (!$scope.hasWriteAccess) {
                _.remove($rootScope.page.subnav.buttons, function (o:any) {
                    return o.text === "Fjern anvendelse";
                });
            }
        }
    ]);
})(angular, app);
