(function(ng, app) {
    app.config(['$stateProvider', function($stateProvider) {
        $stateProvider.state('it-system.usage.advice', {
            url: '/advice/:type',
            templateUrl: 'app/components/it-advice.view.html',
            controller: 'object.EditAdviceCtrl',
            controllerAs: 'Vm',
            resolve: {
                Roles: ['$http', function ($http) {
                  
                    return $http.get("odata/LocalItContractRoles?$filter=IsLocallyAvailable eq true or IsObligatory eq true")
                        .then(function (result) {
                            return result.data.value;
                        });
                }],
                advices: ['$http', '$stateParams', function ($http, $stateParams) {
                    
                    return $http.get('api/itProject/' + $stateParams.id).then(function (result) {
                        return result.data.response.advices;
                        });
                }]
            }
        });
    }]);
})(angular, app);
