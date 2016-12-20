(function(ng, app) {
    app.config(['$stateProvider', function($stateProvider) {
        $stateProvider.state('it-system.usage.advice', {
            url: '/advice/:type',
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
                    return $http.get('api/itSystem/' + $stateParams.id).then(function (result) {
                        return result.data.response.advices;
                        });
                }],
                object: ['itSystemUsage', function (itSystemUsage) {
                    return itSystemUsage;
                }],
                users: ['UserGetService', function (UserGetService) {
                    return UserGetService.GetAllUsers();
                }],
                type: [function () {
                    return "itSystemUsage";
                }]
            }
        });
    }]);
})(angular, app);
