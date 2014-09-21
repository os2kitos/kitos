(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.interface-edit', {
            url: '/edit/{id:[0-9]+}',
            templateUrl: 'partials/it-system/edit-it-interface.html',
            controller: 'system.interfaceEditCtrl',
            resolve: {
                itInterface: ['$http', '$stateParams', function ($http, $stateParams) {
                    var interfaceId = $stateParams.id;
                    return $http.get('api/itInterface/' + interfaceId)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                hasWriteAccess: ['$http', '$stateParams', function ($http, $stateParams) {
                    var interfaceId = $stateParams.id;
                    return $http.get('api/itInterface/' + interfaceId + '?hasWriteAccess')
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('system.interfaceEditCtrl',
    [
        '$rootScope', '$scope', '$http', '$state', 'notify', 'itInterface', 'hasWriteAccess', 'autofocus',
        function ($rootScope, $scope, $http, $state, notify, itInterface, hasWriteAccess, autofocus) {
            $rootScope.page.title = 'Snitflade - Rediger';
            autofocus();

            itInterface.updateUrl = 'api/itInterface/' + itInterface.id;
            $scope.interface = itInterface;
            $scope.hasWriteAccess = hasWriteAccess;
            $scope.select2AllowClearOpt = {
                allowClear: true
            };
        }
    ]);
})(angular, app);
