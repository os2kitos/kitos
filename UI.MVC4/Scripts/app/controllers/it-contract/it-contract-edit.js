(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract.edit', {
            url: '/edit/{id:[0-9]+}',
            templateUrl: 'partials/it-contract/it-contract-edit.html',
            controller: 'contract.EditCtrl',
            resolve: {
                contract: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get('api/itcontract/' + $stateParams.id).then(function (result) {
                        return result.data.response;
                    });
                }],
                contractTypes: ['$http', function ($http) {
                    return $http.get('api/contracttype/').then(function (result) {
                        return result.data.response;
                    });
                }],
                contractTemplates: ['$http', function ($http) {
                    return $http.get('api/contracttemplate/').then(function (result) {
                        return result.data.response;
                    });
                }],
                purchaseForms: ['$http', function ($http) {
                    return $http.get('api/purchaseform/').then(function (result) {
                        return result.data.response;
                    });
                }],
                procurementStrategies: ['$http', function ($http) {
                    return $http.get('api/procurementStrategy/').then(function (result) {
                        return result.data.response;
                    });
                }],
                hasWriteAccess: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itcontractright/" + $stateParams.id + "?hasWriteAccess")
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('contract.EditCtrl', ['$scope', '$http', '$stateParams', 'contract', 'contractTypes', 'contractTemplates', 'purchaseForms', 'procurementStrategies', 'hasWriteAccess',
            function ($scope, $http, $stateParams, contract, contractTypes, contractTemplates, purchaseForms, procurementStrategies, hasWriteAccess) {
                $scope.autosaveUrl = 'api/itcontract/' + $stateParams.id;
                $scope.contract = contract;
                $scope.contractTypes = contractTypes;
                $scope.contractTemplates = contractTemplates;
                $scope.purchaseForms = purchaseForms;
                $scope.procurementStrategies = procurementStrategies;
                $scope.hasWriteAccess = hasWriteAccess;
            }]);
})(angular, app);