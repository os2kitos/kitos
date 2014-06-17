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
                }],
                agreementElements: ['$http', function($http) {
                    return $http.get('api/agreementelement/').then(function (result) {
                        return result.data.response;
                    });
                }],
                customAgreementElements: ['$http', function ($http) {
                    return $http.get('api/customagreementelement/').then(function (result) {
                        return result.data.response;
                    });
                }],
                hasWriteAccess: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itcontract/" + $stateParams.id + "?hasWriteAccess")
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                user: ['userService', function (userService) {
                    return userService.getUser().then(function (user) {
                            return user;
                        });
                }]
            }
        });
    }]);

    app.controller('contract.EditCtrl',
        ['$scope', '$http', '$stateParams', 'notify', 'contract', 'contractTypes', 'contractTemplates', 'purchaseForms', 'procurementStrategies', 'suppliers', 'orgUnits', 'contracts', 'agreementElements', 'customAgreementElements', 'hasWriteAccess', 'user',
            function ($scope, $http, $stateParams, notify, contract, contractTypes, contractTemplates, purchaseForms, procurementStrategies, suppliers, orgUnits, contracts, agreementElements, customAgreementElements, hasWriteAccess, user) {
                $scope.autoSaveUrl = 'api/itcontract/' + $stateParams.id;
                $scope.contract = contract;              
                $scope.hasWriteAccess = hasWriteAccess;
                
                $scope.contractTypes = contractTypes;
                $scope.contractTemplates = contractTemplates;
                $scope.purchaseForms = purchaseForms;
                $scope.procurementStrategies = procurementStrategies;
                $scope.suppliers = suppliers;
                $scope.orgUnits = orgUnits;
                $scope.contracts = contracts;
                $scope.agreementElements = agreementElements; // all available elements
                $scope.customAgreementElements = customAgreementElements;

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
                    patch(payload, $scope.autoSaveUrl);
                };

                function patch(payload, url) {
                    var msg = notify.addInfoMessage("Gemmer...", false);
                    $http({ method: 'PATCH', url: url, data: payload })
                        .success(function () {
                            msg.toSuccessMessage("Feltet er opdateret.");
                        })
                        .error(function () {
                            msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                        });
                }

                // check selected agreementElements
                (function () {
                    var selectedIds = _.pluck(contract.agreementElements, 'id'); // selected agreementElements
                    _.each(selectedIds, function(id) {
                        var foundElem = _.find($scope.agreementElements, function(elem) {
                            return elem.id == id;
                        });
                        if (foundElem) {
                            foundElem.isChecked = true;
                        }
                    });
                })();
                
                $scope.updateElem = function(item) {
                    var msg = notify.addInfoMessage("Gemmer...", false);
                    if (item.isChecked) {
                        $http.post('api/itcontract/' + $stateParams.id + '?elemId=' + item.id)
                            .success(function() {
                                msg.toSuccessMessage("Feltet er oprettet.");
                            })
                            .error(function() {
                                msg.toErrorMessage("Fejl! Feltet kunne ikke oprettes!");
                            });
                    } else {
                        $http.delete('api/itcontract/' + $stateParams.id + '?elemId=' + item.id)
                            .success(function () {
                                msg.toSuccessMessage("Feltet er oprettet.");
                            })
                            .error(function () {
                                msg.toErrorMessage("Fejl! Feltet kunne ikke oprettes!");
                            });
                    }
                };

                function clearNewAgreementItem() {
                    $scope.newItem = {};
                }

                clearNewAgreementItem();

                $scope.saveCustomElem = function() {
                    var payload = {
                        name: $scope.newItem.name,
                        itContractId: $stateParams.id
                    };
                    var msg = notify.addInfoMessage("Gemmer...", false);
                    $http.post('api/customagreementelement', payload)
                        .success(function(result) {
                            msg.toSuccessMessage("Feltet er oprettet.");
                            $scope.customAgreementElements.push(result.response);
                            clearNewAgreementItem();
                        })
                        .error(function() {
                            msg.toErrorMessage("Fejl! Feltet kunne ikke oprettes!");
                        });
                };

                $scope.deleteCustomElem = function(id) {
                    var msg = notify.addInfoMessage("Sletter...", false);
                    $http.delete('api/customagreementelement/' + id)
                        .success(function() {
                            msg.toSuccessMessage("Feltet er slettet.");
                            var foundElem = _.find($scope.customAgreementElements, function (elem) {
                                return elem.id == id;
                            });
                            if (foundElem) {
                                var index = _.indexOf($scope.customAgreementElements, foundElem);
                                $scope.customAgreementElements.splice(index, 1);
                            }
                        })
                        .error(function() {
                            msg.toErrorMessage("Fejl! Feltet kunne ikke slettes!");
                        });
                };

                if (contract.parentId) {
                    $scope.contract.parent = {
                        id: contract.parentId,
                        text: contract.parentName
                    };
                }
                
                $scope.itContractsSelectOptions = selectLazyLoading('api/itcontract', true, formatContract, ['orgId=' + user.currentOrganizationId]);

                function formatContract(supplier) {
                    return '<div>' + supplier.text + '</div>';
                }

                if (contract.supplierId) {
                    $scope.contract.supplier = {
                        id: contract.supplierId,
                        text: contract.supplierName
                    };
                }
                
                $scope.suppliersSelectOptions = selectLazyLoading('api/organization', false, formatSupplier, ['public']);

                function formatSupplier(supplier) {
                    var result = '<div>' + supplier.text + '</div>';
                    if (supplier.cvr) {
                        result += '<div class="small">' + supplier.cvr + '</div>';
                    }
                    return result;
                }

                function selectLazyLoading(url, excludeSelf, format, paramAry) {
                    return {
                        minimumInputLength: 1,
                        allowClear: true,
                        placeholder: ' ',
                        formatResult: format,
                        initSelection: function(elem, callback) {
                        },
                        ajax: {
                            data: function(term, page) {
                                return { query: term };
                            },
                            quietMillis: 500,
                            transport: function(queryParams) {
                                var extraParams = paramAry ? '&' + paramAry.join('&') : '';
                                var res = $http.get(url + '?q=' + queryParams.data.query + extraParams).then(queryParams.success);
                                res.abort = function() {
                                    return null;
                                };

                                return res;
                            },

                            results: function(data, page) {
                                var results = [];

                                _.each(data.data.response, function(obj) {
                                    if (excludeSelf && obj.id == contract.id)
                                        return; // don't add self to result
                                    
                                    results.push({
                                        id: obj.id,
                                        text: obj.name ? obj.name : 'Unavngiven',
                                        cvr: obj.cvr
                                    });
                                });

                                return { results: results };
                            }
                        }
                    };
                }

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

                $scope.opened = {};
                $scope.open = function ($event, datepicker) {
                    $event.preventDefault();
                    $event.stopPropagation();
                    
                    $scope.opened[datepicker] = true;
                };
            }]);
})(angular, app);