(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.edit.paymentmodel', {
            url: '/paymentmodel',
            templateUrl: 'app/components/it-contract/tabs/it-contract-tab-paymentmodel.view.html',
            controller: 'contract.PaymentmodelCtrl',
            resolve: {
                paymentFrequencies: ['$http', function ($http) {
                    return $http.get('odata/LocalPaymentFrequencyTypes?$filter=IsActive+eq+true').then(function (result) {
                        return result.data.value;
                    });
                }],
                paymentModels: ['$http', function ($http) {
                    return $http.get('odata/LocalPaymentModelTypes?$filter=IsActive+eq+true').then(function (result) {
                        return result.data.value;
                    });
                }],
                priceRegulations: ['$http', function ($http) {
                    return $http.get('odata/LocalPriceRegulationTypes?$filter=IsActive+eq+true').then(function (result) {
                        return result.data.value;
                    });
                }]
            }
        });
    }]);

    app.controller('contract.PaymentmodelCtrl', ['$scope', '$http', 'notify', 'contract', 'paymentFrequencies', 'paymentModels', 'priceRegulations',
            function ($scope, $http, notify, contract, paymentFrequencies, paymentModels, priceRegulations) {
                $scope.contract = contract;
                $scope.autosaveUrl = 'api/itcontract/' + contract.id;
                $scope.paymentFrequencies = paymentFrequencies;
                $scope.paymentModels = paymentModels;
                $scope.priceRegulations = priceRegulations;

                $scope.datepickerOptions = {
                    format: "dd-MM-yyyy",
                    parseFormats: ["yyyy-MM-dd"]
                };
            }]);
})(angular, app);
