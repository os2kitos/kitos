(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.edit.advice-generic', {
            url: '/advice',
            templateUrl: 'app/components/it-advice/it-advice.view.html',
            controller: 'object.EditAdviceCtrl',
            controllerAs: 'Vm',
            resolve: {
                Roles: ["localOptionServiceFactory", (localOptionServiceFactory : Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                    localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.ItContractRoles).getAll()], 
                advices: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get('api/itcontract/' + $stateParams.id).then(function (result) {
                        return result.data.response.advices;
                    });
                }],
                object: ['contract', function (contract) {
                    return contract;
                }],
                type: [function () {
                    return "itContract";
                }],
                advicename: [
                    '$http', '$stateParams', function ($http, $stateParams) {
                        return $http.get('api/itcontract/' + $stateParams.id).then(function (result) {
                            return result.data.response;
                        });
                    }
                ]
            }
        });
    }]);
})(angular, app);
