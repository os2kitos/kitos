(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.interface-edit.advice', {
            url: '/advice',
            templateUrl: 'app/components/it-advice/it-advice.view.html',
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
                        return $http.get('api/itInterface/' + interfaceId)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }
                ],
                type: [function () {
                    return "itInterface";
                }],
                hasWriteAccess: [
                    '$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                        return $http.get("api/itInterface/" + $stateParams.id + "?hasWriteAccess=true&organizationId=" + user.currentOrganizationId)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }],
                advicename: [
                    '$http', '$stateParams', function ($http, $stateParams) {
                        var interfaceId = $stateParams.id;
                        return $http.get('api/itInterface/' + interfaceId)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }
                ]
            }
        });
    }]);
})(angular, app);