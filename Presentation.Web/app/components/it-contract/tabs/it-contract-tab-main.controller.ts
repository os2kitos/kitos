(function (ng, app) {
    app.config([
        '$stateProvider', function ($stateProvider) {
            $stateProvider.state('it-contract.edit.main', {
                url: '/main',
                templateUrl: 'app/components/it-contract/tabs/it-contract-tab-main.view.html',
                controller: 'contract.EditMainCtrl',
                resolve: {
                    contractTypes: [
                        'localOptionServiceFactory', (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                        localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.ItContractTypes).getAll()
                    ],
                    contractTemplates: [
                        'localOptionServiceFactory', (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                        localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.ItContractTemplateTypes).getAll()
                    ],
                    purchaseForms: [
                        "localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                        localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.PurchaseFormTypes).getAll()
                    ],
                    procurementStrategies: [
                        "localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                        localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.ProcurementStrategyTypes).getAll()
                    ],
                    orgUnits: [
                        '$http', 'contract', function ($http, contract) {
                            return $http.get('api/organizationunit/?organizationid=' + contract.organizationId).then(function (result) {
                                return result.data.response;
                            });
                        }
                    ],
                    kitosUsers: [
                        '$http', 'user', '_', function ($http, user, _) {
                            return $http.get(`odata/Organizations(${user.currentOrganizationId})/Rights?$expand=User($select=Name,LastName,Email,Id)&$select=User`).then(function (result) {
                                let uniqueUsers = _.uniqBy(result.data.value, "User.Id");

                                let results = [];
                                _.forEach(uniqueUsers, data => {
                                    results.push({
                                        Name: data.User.Name,
                                        LastName: data.User.LastName,
                                        Email: data.User.Email,
                                        Id: data.User.Id
                                    });
                                });
                                results = _.orderBy(results, x => x.Name, 'asc');
                                return results;
                            });
                        }
                    ]
                }
            });
        }
    ]);

    app.controller('contract.EditMainCtrl',
        [
            '$scope', '$http', '_', '$stateParams', '$uibModal', 'notify', 'contract', 'contractTypes', 'contractTemplates', 'purchaseForms', 'procurementStrategies', 'orgUnits', 'hasWriteAccess', 'user', 'autofocus', '$timeout', 'kitosUsers',
            function ($scope, $http, _, $stateParams, $uibModal, notify, contract, contractTypes, contractTemplates, purchaseForms, procurementStrategies, orgUnits, hasWriteAccess, user : Kitos.Services.IUser, autofocus, $timeout, kitosUsers) {

                $scope.autoSaveUrl = 'api/itcontract/' + $stateParams.id;
                $scope.autosaveUrl2 = 'api/itcontract/' + contract.id;
                $scope.contract = contract;
                $scope.hasWriteAccess = hasWriteAccess;
                $scope.hasViewAccess = user.currentOrganizationId == contract.organizationId;
                $scope.kitosUsers = kitosUsers;
                autofocus();
                $scope.contractTypes = contractTypes;
                $scope.contractTemplates = contractTemplates;
                $scope.purchaseForms = purchaseForms;
                $scope.procurementStrategies = procurementStrategies;
                $scope.orgUnits = orgUnits;
                var today = new Date();
                $scope.dataHandlerLink = '';
                
                if ($scope.contract.dataHandler != null) {
                    $scope.dataHandlerLink = '#/contract/edit/' + $scope.contract.dataHandlerId + '/main';

                    if (!$scope.contract.dataHandlerAgreementUrlName) {
                        $scope.dataHandlerLinkName = $scope.contract.dataHandler.name;
                    } else {
                        $scope.dataHandlerLinkName = $scope.contract.dataHandlerAgreementUrlName;
                    }

                } else if ($scope.contract.dataHandlerAgreementUrl != '') {
                    $scope.dataHandlerLink = $scope.contract.dataHandlerAgreementUrl;

                    if (!$scope.contract.dataHandlerAgreementUrlName) {
                        $scope.dataHandlerLinkName = "Databehandleraftale";
                    } else {
                        $scope.dataHandlerLinkName = $scope.contract.dataHandlerAgreementUrlName;
                    }
                }

                if (!contract.active) {
                    if (contract.concluded < today && today < contract.expirationDate) {
                        $scope.displayActive = true;
                    } else {
                        $scope.displayActive = false;
                    }
                } else {
                    $scope.displayActive = false;
                }

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

                var foundPlan: { id } = _.find($scope.procurementPlans, function (plan: { half; year; id; }) {
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
                $scope.patchDate = (field, value) => {
                    var date = moment(moment(value, "DD-MM-YYYY", true).format());
                    if(value === "") {
                        var payload = {};
                        payload[field] = null;
                        patch(payload, $scope.autosaveUrl2 + '?organizationId=' + user.currentOrganizationId);
                    } else if (value == null) {

                    } else if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                        notify.addErrorMessage("Den indtastede dato er ugyldig.");

                    } else {
                        var dateString = date.format("YYYY-MM-DD");
                        var payload = {};
                        payload[field] = dateString;
                        patch(payload, $scope.autosaveUrl2 + '?organizationId=' + user.currentOrganizationId);
                    }
                }

                $scope.saveProcurement = function () {
                    var payload;
                    // if empty the value has been cleared
                    if ($scope.contract.procurementPlan === '') {
                        contract = $scope.contract; 
                        payload = { procurementPlanHalf: null, procurementPlanYear: null };
                    } else {
                        var id = $scope.contract.procurementPlan;
                        var result = $scope.procurementPlans[id];
                        payload = { procurementPlanHalf: result.half, procurementPlanYear: result.year };
                    }

                    $scope.contract.procurementPlanHalf = payload.procurementPlanHalf;
                    $scope.contract.procurementPlanYear = payload.procurementPlanYear;
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

                function formatContract(supplier) {
                    return '<div>' + supplier.text + '</div>';
                }

                if (contract.supplierId) {
                    $scope.contract.supplier = {
                        id: contract.supplierId,
                        text: contract.supplierName
                    };
                }

                $scope.suppliersSelectOptions = selectLazyLoading('api/organization', false, formatSupplier, ['take=25','orgId=' + user.currentOrganizationId]);

                function formatSupplier(supplier) {
                    var result = '<div>' + supplier.text + '</div>';
                    if (supplier.cvr) {
                        result += '<div class="small">' + supplier.cvr + '</div>';
                    }
                    return result;
                }

                $scope.editLink = function () {
                    $uibModal.open({
                        templateUrl: 'app/components/it-contract/tabs/it-contract-gdpr-editlink-modal.view.html',
                        controller: ['$scope', '$state', '$uibModalInstance', 'contracts', 'contract', function ($scope, $state, $uibModalInstance, contracts, contract) {
                            $scope.contracts = contracts
                            $scope.contract = contract

                            if ($scope.contract.dataHandlerId == 0){
                                $scope.contract.dataHandlerId = "";
                            }
                            
                            $scope.ok = function () {
                                
                                var msg = notify.addInfoMessage("Gemmer...", false);

                                var payload = {
                                    Id: contract.id,
                                    DataHandlerId: $scope.contract.dataHandlerId,
                                    DataHandlerAgreementUrl: $scope.contract.dataHandlerAgreementUrl,
                                    DataHandlerAgreementUrlName: $scope.contract.dataHandlerAgreementUrlName
                                }

                                $http({ method: 'PATCH', url: 'api/itcontract/' + contract.id + '?organizationId=' + user.currentOrganizationId, data: payload })
                                    .success(function () {
                                        msg.toSuccessMessage("Feltet er opdateret.");
                                        $state.reload();
                                    })
                                    .error(function () {
                                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                    });
                                $uibModalInstance.close();
                            };

                            $scope.cancel = function () {
                                $uibModalInstance.dismiss('cancel');
                            };
                        }],
                        resolve: {
                            contracts: [
                                '$http', function ($http) {
                                    return $http.get(`odata/Organizations(${user.currentOrganizationId})/ItContracts?$filter=Active eq true`).then(function (result) {
                                        return result.data.value;
                                    });
                                }],
                                contract: [ function () {
                                    return $scope.contract;
                                    }
                            ]
                        }
                    })
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
                $scope.override = () =>
                {
                    isActive();
                }
                function isActive() {
                    var today = moment();
                    let fromDate = moment($scope.contract.concluded, "DD-MM-YYYY").startOf('day');
                    let endDate = moment($scope.contract.expirationDate, "DD-MM-YYYY").endOf('day');
                    if ($scope.contract.active || today.isBetween(fromDate, endDate, null, '[]') ||
                        (today.isSameOrAfter(fromDate) && !endDate.isValid()) ||
                        (today.isSameOrBefore(endDate) && !fromDate.isValid()) ||
                        (!fromDate.isValid() && !endDate.isValid())) {
                        $scope.contract.isActive = true;
                    }
                    else {
                        $scope.contract.isActive = false;
                    }
                }
                $scope.checkContractValidity = (field, value) => {
                    var expirationDate = $scope.contract.expirationDate;
                    var concluded = $scope.contract.concluded;
                    var formatString = "DD-MM-YYYY";
                    var formatDateString = "YYYY-MM-DD";
                    var fromDate = moment(concluded, [formatString, formatDateString]).startOf('day');
                    var endDate = moment(expirationDate, [formatString, formatDateString]).endOf('day');
                    var date = moment(value, ["DD-MM-YYYY", "YYYY-MM-DDTHH:mm:ssZ"], true);
                    var payload = {};
                    if (value === "") {
                        payload[field] = null;
                        patch(payload, $scope.autosaveUrl2 + '?organizationId=' + user.currentOrganizationId);
                        isActive();
                    }
                    else if (value == null) {
                        //made to prevent error message on empty value i.e. open close datepicker
                    }
                    else if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                        notify.addErrorMessage("Den indtastede dato er ugyldig.");
                    }
                    else if (fromDate >= endDate) {
                        notify.addErrorMessage("Den indtastede slutdato er før startdatoen.");
                    }
                    else {
                        var dateString = date.format("YYYY-MM-DD");
                        payload[field] = dateString;
                        patch(payload, $scope.autosaveUrl2 + '?organizationId=' + user.currentOrganizationId);
                        isActive();
                    }
                }
            }]);
})(angular, app);
