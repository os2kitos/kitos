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
        ['$scope', '$http', '$stateParams', 'notify', 'contract', 'contractTypes', 'contractTemplates', 'purchaseForms', 'procurementStrategies', 'suppliers', 'orgUnits', 'contracts',
            function ($scope, $http, $stateParams, notify, contract, contractTypes, contractTemplates, purchaseForms, procurementStrategies, suppliers, orgUnits, contracts) {
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

                var foundPlan = _.find($scope.procurementPlans, function(plan) {
                    return plan.half == contract.procurementPlanHalf && plan.year == contract.procurementPlanYear;
                });
                if (foundPlan) {
                    // plan is found in the list, replace it to get object equality
                    $scope.contract.procurementPlan = foundPlan;
                } else {
                    // plan is not found, add missing plan to begining of list
                    $scope.procurementPlans.unshift({ half: contract.procurementPlanHalf, year: contract.procurementPlanYear });
                }

                $scope.saveProcurement = function() {
                    var payload = { procurementPlanHalf: $scope.contract.procurementPlan.half, procurementPlanYear: $scope.contract.procurementPlan.year };
                    save(payload);
                };

                function save(payload) {
                    var msg = notify.addInfoMessage("Gemmer...", false);
                    $http({ method: 'PATCH', url: $scope.autoSaveUrl, data: payload })
                        .success(function () {
                            msg.toSuccessMessage("Feltet er opdateret.");
                        })
                        .error(function () {
                            msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                        });
                }
            }
        ]
    );
})(angular, app);