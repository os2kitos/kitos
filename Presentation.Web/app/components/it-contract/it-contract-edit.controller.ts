(function (ng, app) {
    app.config([
        '$stateProvider', function ($stateProvider) {
            $stateProvider.state('it-contract.edit', {
                url: '/edit/{id:[0-9]+}',
                templateUrl: 'app/components/it-contract/it-contract-edit.view.html',
                controller: 'contract.EditCtrl',
                resolve: {
                    contract: [
                        '$http', '$stateParams', function ($http, $stateParams) {
                            return $http.get('api/itcontract/' + $stateParams.id).then(function (result) {
                                return result.data.response;
                            });
                        }
                    ],
                    contractTypes: [
                        '$http', function ($http) {
                            return $http.get('api/contracttype/').then(function (result) {
                                return result.data.response;
                            });
                        }
                    ],
                    contractTemplates: [
                        '$http', function ($http) {
                            return $http.get('api/contracttemplate/').then(function (result) {
                                return result.data.response;
                            });
                        }
                    ],
                    purchaseForms: [
                        '$http', function ($http) {
                            return $http.get('api/purchaseform/').then(function (result) {
                                return result.data.response;
                            });
                        }
                    ],
                    procurementStrategies: [
                        '$http', function ($http) {
                            return $http.get('api/procurementStrategy/').then(function (result) {
                                return result.data.response;
                            });
                        }
                    ],
                    orgUnits: [
                        '$http', 'contract', function ($http, contract) {
                            return $http.get('api/organizationunit/?organizationid=' + contract.organizationId).then(function (result) {
                                return result.data.response;
                            });
                        }
                    ],
                    contracts: [
                        '$http', function ($http) {
                            return $http.get('api/itcontract/').then(function (result) {
                                return result.data.response;
                            });
                        }
                    ],
                    agreementElements: [
                        '$http', function ($http) {
                            return $http.get('api/agreementelement/').then(function (result) {
                                return result.data.response;
                            });
                        }
                    ],
                    hasWriteAccess: [
                        '$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                            return $http.get("api/itcontract/" + $stateParams.id + "?hasWriteAccess=true&organizationId=" + user.currentOrganizationId)
                                .then(function (result) {
                                    return result.data.response;
                                });
                        }
                    ],
                    user: [
                        'userService', function (userService) {
                            return userService.getUser().then(function (user) {
                                return user;
                            });
                        }
                    ]
                }
            });
        }
    ]);

    app.controller('contract.EditCtrl',
    [
        '$scope', '$http', '$stateParams', 'notify', 'contract', 'contractTypes', 'contractTemplates', 'purchaseForms', 'procurementStrategies', 'orgUnits', 'contracts', 'agreementElements', 'hasWriteAccess', 'user', 'autofocus',
        function ($scope, $http, $stateParams, notify, contract, contractTypes, contractTemplates, purchaseForms, procurementStrategies, orgUnits, contracts, agreementElements, hasWriteAccess, user, autofocus) {
            $scope.autoSaveUrl = 'api/itcontract/' + $stateParams.id;
            $scope.contract = contract;
            $scope.hasWriteAccess = hasWriteAccess;

            autofocus();

            $scope.contractTypes = contractTypes;
            $scope.contractTemplates = contractTemplates;
            $scope.purchaseForms = purchaseForms;
            $scope.procurementStrategies = procurementStrategies;
            $scope.orgUnits = orgUnits;
            $scope.contracts = contracts;
            $scope.agreementElements = agreementElements;
            $scope.selectedAgreementElementIds = _.map(contract.agreementElements, 'id');
            $scope.selectedAgreementElementNames = _.map(contract.agreementElements, 'name');

            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            $scope.procurementPlans = [];
            var currentDate = moment();
            for (var i = 0; i < 20; i++) {
                var half = currentDate.quarter() <= 2 ? 1 : 2; // 1 for the first 6 months, 2 for the rest
                var year = currentDate.year();
                var obj = { id: i, half: half, year: year };
                $scope.procurementPlans.push(obj);

                // add 6 months for next iter
                currentDate.add(6, 'months');
            }

            var foundPlan: { id } = _.find($scope.procurementPlans, function (plan: {half; year; id; }) {
                return plan.half == contract.procurementPlanHalf && plan.year == contract.procurementPlanYear;
            });
            if (foundPlan) {
                // plan is found in the list, replace it to get object equality
                $scope.contract.procurementPlan = foundPlan.id;
            } else {
                // plan is not found, add missing plan to begining of list
                // if not null
                if (contract.procurementPlanHalf != null) {
                    var plan = { id: $scope.procurementPlans.length, half: contract.procurementPlanHalf, year: contract.procurementPlanYear };
                    $scope.procurementPlans.unshift(plan); // add to list
                    $scope.contract.procurementPlan = plan.id; // select it
                }
            }

            $scope.saveProcurement = function () {
                var payload;
                // if empty the value has been cleared
                if ($scope.contract.procurementPlan === '') {
                    payload = { procurementPlanHalf: null, procurementPlanYear: null };
                } else {
                    var id = $scope.contract.procurementPlan;
                    var result = $scope.procurementPlans[id];
                    payload = { procurementPlanHalf: result.half, procurementPlanYear: result.year };
                }
                patch(payload, $scope.autoSaveUrl + '?organizationId=' + user.currentOrganizationId);
            };

            $scope.procurementPlanOption = {
                allowClear: true,
                initSelection: function (element, callback) {
                    callback({ id: 1, text: 'Text' });
                }
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

            if (contract.parentId) {
                $scope.contract.parent = {
                    id: contract.parentId,
                    text: contract.parentName
                };
            }

            $scope.itContractsSelectOptions = selectLazyLoading('api/itcontract', true, formatContract, ['orgId=' + user.currentOrganizationId]);

            $scope.allowClearOption = {
                allowClear: true
            };

            function formatContract(supplier) {
                return '<div>' + supplier.text + '</div>';
            }

            if (contract.supplierId) {
                $scope.contract.supplier = {
                    id: contract.supplierId,
                    text: contract.supplierName
                };
            }

            $scope.suppliersSelectOptions = selectLazyLoading('api/organization', false, formatSupplier, ['public=true', 'orgId=' + user.currentOrganizationId]);

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
                    initSelection: function (elem, callback) {
                    },
                    ajax: {
                        data: function (term, page) {
                            return { query: term };
                        },
                        quietMillis: 500,
                        transport: function (queryParams) {
                            var extraParams = paramAry ? '&' + paramAry.join('&') : '';
                            var res = $http.get(url + '?q=' + queryParams.data.query + extraParams).then(queryParams.success);
                            res.abort = function () {
                                return null;
                            };

                            return res;
                        },

                        results: function (data, page) {
                            var results = [];

                            _.each(data.data.response, function (obj: { id; name; cvr; }) {
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
                        text: signer.fullName
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
                            url: 'api/itcontract/' + contract.id + '?organizationId=' + user.currentOrganizationId,
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
