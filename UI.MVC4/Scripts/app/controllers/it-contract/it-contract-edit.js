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
                suppliers: ['$http', function ($http) {
                    return $http.get('api/organization/?company').then(function (result) {
                        return result.data.response;
                    });
                }],
                orgUnits: ['$http', 'contract', function ($http, contract) {
                    return $http.get('api/organizationunit/?organizationid=' + contract.organizationId).then(function (result) {
                        return result.data.response;
                    });
                }],
                contracts: ['$http', function($http) {
                    return $http.get('api/itcontract/').then(function (result) {
                        return result.data.response;
                    });
                }]
            }
        });
    }]);

    app.controller('contract.EditCtrl',
        ['$scope', '$http', '$stateParams', 'contract', 'contractTypes', 'contractTemplates', 'purchaseForms', 'procurementStrategies', 'suppliers', 'orgUnits', 'contracts',
            function ($scope, $http, $stateParams, contract, contractTypes, contractTemplates, purchaseForms, procurementStrategies, suppliers, orgUnits, contracts) {
                $scope.autoSaveUrl = 'api/itcontract/' + $stateParams.id;
                $scope.contract = contract;
                $scope.contractTypes = contractTypes;
                $scope.contractTemplates = contractTemplates;
                $scope.purchaseForms = purchaseForms;
                $scope.procurementStrategies = procurementStrategies;
                $scope.suppliers = suppliers;
                $scope.orgUnits = orgUnits;
                $scope.contracts = contracts;

                $scope.procurementPlans = [];
                var currentDate = moment();
                for (var i = 0; i < 20; i++) {
                    var half = Math.ceil(currentDate.month() / 6); // calcs 1 for the first 6 months, 2 for the rest
                    var year = currentDate.year();
                    var obj = { half: half, year: year };
                    $scope.procurementPlans.push(obj);
                    
                    // add 6 months for next iter
                    currentDate.add('months', 6);
                }
            }
        ]
    );
})(angular, app);