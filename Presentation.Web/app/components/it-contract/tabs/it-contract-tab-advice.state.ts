(function(ng, app) {
    app.config(['$stateProvider', function($stateProvider) {
        $stateProvider.state('it-contract.edit.advice-generic', {
            url: '/advice/:id/:type',
            templateUrl: 'app/components/it-contract/tabs/it-contract-tab-advice.view.html',
            controller: 'object.EditAdviceCtrl',
            resolve: {
                
                itContractRoles: ['$http', function ($http) {
                  
                    return $http.get("odata/LocalItContractRoles?$filter=IsLocallyAvailable eq true or IsObligatory eq true")
                        .then(function (result) {
                            return result.data.value;
                        });
                }],
                advices: ['$http', '$stateParams', function ($http, $stateParams) {
                    if ($stateParams.id != undefined) { 
                    return $http.get('api/itcontract/' + $stateParams.id).then(function (result) {
                        return result.data.response.advices;
                        });
                    }
                    return null;
                }]
            }
        });
    }]);

})(angular, app);
