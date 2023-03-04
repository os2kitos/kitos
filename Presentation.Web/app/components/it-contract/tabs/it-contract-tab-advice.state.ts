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
                object: ['contract', function (contract) {
                    return contract;
                }],
                type: [function () {
                    return Kitos.Models.Advice.AdviceType.ItContract;
                }],
                advicename: [
                    '$http', '$stateParams', function ($http, $stateParams) {
                        return $http.get('api/itcontract/' + $stateParams.id).then(function (result) {
                            return result.data.response;
                        });
                    }
                ],
                currentUser: [
                    "userService",
                    (userService: Kitos.Services.IUserService) => userService.getUser()
                ]
            }
        });
    }]);
})(angular, app);
