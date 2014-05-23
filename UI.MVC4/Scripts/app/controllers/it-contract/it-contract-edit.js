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

    app.controller('contract.EditCtrl', ['$scope', '$http', '$stateParams', 'notify', 'contract', 'contractTypes', 'contractTemplates', 'purchaseForms', 'procurementStrategies', 'hasWriteAccess',
            function ($scope, $http, $stateParams, notify, contract, contractTypes, contractTemplates, purchaseForms, procurementStrategies, hasWriteAccess) {
                $scope.autosaveUrl = 'api/itcontract/' + $stateParams.id;
                $scope.contract = contract;
                $scope.contractTypes = contractTypes;
                $scope.contractTemplates = contractTemplates;
                $scope.purchaseForms = purchaseForms;
                $scope.procurementStrategies = procurementStrategies;
                $scope.hasWriteAccess = hasWriteAccess;
                


                function formatContractSigner(signer) {

                    var userForSelect = null;
                    if (signer) {
                        userForSelect = {
                            id: signer.id,
                            text: signer.name
                        };
                    }

                    $scope.contractSigner = {
                        edit: false,
                        signer: signer,
                        userForSelect: userForSelect,
                        update: function () {
                            var msg = notify.addInfoMessage("Gemmer...", false);

                            var selectedUser = $scope.contractSigner.userForSelect;
                            var signerId = selectedUser ? selectedUser.id : null;
                            var signerUser = selectedUser ? selectedUser.user : null;
                            
                            console.log($scope.contractSigner);

                            $http({
                                method: 'PATCH',
                                url: 'api/itContract/' + contract.id,
                                data: {
                                    contractSignerId: signerId
                                }
                            }).success(function (result) {

                                msg.toSuccessMessage("Kontraktunderskriveren er gemt");

                                formatContractSigner(signerUser);

                            }).error(function () {
                                msg.toErrorMessage("Fejl!");
                            });
                        }
                    };


                }

                formatContractSigner(contract.contractSigner);
            }]);
})(angular, app);