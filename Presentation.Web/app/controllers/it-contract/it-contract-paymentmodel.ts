(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.edit.paymentmodel', {
            url: '/paymentmodel',
            templateUrl: 'partials/it-contract/tab-paymentmodel.html',
            controller: 'contract.PaymentmodelCtrl',
            resolve: {
                paymentFrequencies: ['$http', function ($http) {
                    return $http.get('api/paymentFrequency').then(function (result) {
                        return result.data.response;
                    });
                }],
                paymentModels: ['$http', function ($http) {
                    return $http.get('api/paymentModel').then(function (result) {
                        return result.data.response;
                    });
                }],
                priceRegulations: ['$http', function ($http) {
                    return $http.get('api/priceRegulation').then(function (result) {
                        return result.data.response;
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
