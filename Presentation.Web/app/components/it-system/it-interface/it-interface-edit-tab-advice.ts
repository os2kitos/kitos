(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.interface-edit.advice', {
            url: '/advice/:type',
            templateUrl: 'app/components/it-advice.view.html',
            controller: 'object.EditAdviceCtrl',
            controllerAs: 'Vm',
            resolve: {
                Roles: ['$http', function ($http) {
                    return $http.get("odata/LocalItSystemRoles?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                        .then(function (result) {
                            return result.data.value;
                        });
                }],
                advices: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get('api/itInterface/' + '1').then(function (result) {
                        return result.data.response.advices;
                    });
                }],
                object: [
                    '$http', '$stateParams', function ($http, $stateParams) {
                        var interfaceId = $stateParams.id;
                        return $http.get('api/itInterface/' + '1')
                            .then(function (result) {
                                return result.data.response;
                            });
                    }
                ],
                users: ['UserGetService', function (UserGetService) {
                    return UserGetService.GetAllUsers();
                }],
                type: [function () {
                    return "itInterface";
                }]
            }
        });
    }]);
})(angular, app);