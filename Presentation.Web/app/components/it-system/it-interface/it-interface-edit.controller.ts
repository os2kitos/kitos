(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.interface-edit', {
            url: '/edit/{id:[0-9]+}/interface',
            templateUrl: 'app/components/it-system/it-interface/it-interface-edit.view.html',
            controller: 'system.interfaceEditCtrl',
            resolve: {
                user: [
                    'userService', function (userService) {
                        return userService.getUser();
                    }
                ],
                hasWriteAccess: [
                    '$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                        var interfaceId = $stateParams.id;
                        return $http.get('api/itInterface/' + interfaceId + '?hasWriteAccess=true&organizationId=' + user.currentOrganizationId)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }
                ]
            }
        });
    }]);

    app.controller('system.interfaceEditCtrl',
    [
        '$rootScope', '$scope', 'user', 'hasWriteAccess',
        function ($rootScope, $scope, user, hasWriteAcess) {
            $scope.hasWriteAcess = hasWriteAcess;
        }
    ]);
})(angular, app);
