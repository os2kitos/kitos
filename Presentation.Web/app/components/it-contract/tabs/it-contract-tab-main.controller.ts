(function (ng, app) {
    app.config([
        '$stateProvider', function ($stateProvider) {
            $stateProvider.state('it-contract.edit.main', {
                url: '/main',
                templateUrl: 'app/components/it-contract/tabs/it-contract-tab-main.view.html',
                controller: 'contract.EditMainCtrl',
                resolve: {
                    contractTypes: [
                        'localOptionServiceFactory',
                        (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                            localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.ItContractTypes)
                                .getAll()
                    ],
                    contractTemplates: [
                        'localOptionServiceFactory',
                        (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                            localOptionServiceFactory
                                .create(Kitos.Services.LocalOptions.LocalOptionType.ItContractTemplateTypes).getAll()
                    ],
                    purchaseForms: [
                        "localOptionServiceFactory",
                        (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                            localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.PurchaseFormTypes)
                                .getAll()
                    ],
                    procurementStrategies: [
                        "localOptionServiceFactory",
                        (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                            localOptionServiceFactory
                                .create(Kitos.Services.LocalOptions.LocalOptionType.ProcurementStrategyTypes).getAll()
                    ],
                    orgUnits: [
                        "organizationApiService", "contract", (organizationApiService: Kitos.Services.IOrganizationApiService, contract) =>
                            organizationApiService.getOrganizationUnit(contract.organizationId).then(result =>
                                Kitos.Helpers.Select2OptionsFormatHelper.addIndentationToUnitChildren(result, 0))
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
            '$scope', '$http', '_', '$stateParams',
            'notify', 'contract', 'contractTypes', 'contractTemplates',
            'purchaseForms', 'procurementStrategies', 'orgUnits', 'hasWriteAccess',
            'user', 'autofocus', 'kitosUsers', "uiState",
            "criticalityOptions", "select2LoadingService", "itContractService",
            function ($scope, $http, _, $stateParams,
                notify, contract, contractTypes, contractTemplates,
                purchaseForms, procurementStrategies, orgUnits: Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<number>[], hasWriteAccess,
                user: Kitos.Services.IUser, autofocus, kitosUsers, uiState: Kitos.Models.UICustomization.ICustomizedModuleUI,
                criticalityOptions: Kitos.Models.IOptionEntity[], select2LoadingService: Kitos.Services.ISelect2LoadingService,
                itContractService: Kitos.Services.ItContract.IItContractService) {

                const blueprint = Kitos.Models.UICustomization.Configs.BluePrints.ItContractUiCustomizationBluePrint;

                bindCriticalities(contract);
                $scope.autoSaveUrl = 'api/itcontract/' + $stateParams.id;
                $scope.autosaveUrl2 = 'api/itcontract/' + contract.id;
                $scope.contract = contract;
                $scope.lastChanged = Kitos.Helpers.RenderFieldsHelper.renderDate(contract.lastChanged);
                $scope.hasWriteAccess = hasWriteAccess;
                $scope.kitosUsers = kitosUsers;
                autofocus();
                $scope.contractTypes = contractTypes;
                $scope.contractTemplates = contractTemplates;
                $scope.purchaseForms = purchaseForms;
                $scope.procurementStrategies = procurementStrategies;
                $scope.orgUnits = orgUnits;
                $scope.allowClear = true;

                $scope.showprocurementPlanSelection = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage.children.procurementPlan);
                $scope.showProcurementStrategySelection = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage.children.procurementStrategy);
                $scope.showProcurementInitiated = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage.children.procurementInitiated);
                $scope.isContractIdEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage.children.contractId);
                $scope.isContractTemplateEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage.children.template);
                $scope.isCritialityEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage.children.criticality);
                $scope.isContractTypeEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage.children.contractType);
                $scope.isPurchaseFormEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage.children.purchaseForm);
                $scope.isExternalSignerEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage.children.externalSigner);
                $scope.isInternalSignerEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage.children.internalSigner);
                $scope.isagreementPeriodEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage.children.agreementPeriod);
                $scope.isActiveEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage.children.isActive);

                bindProcurementInitiated();
                reloadValidationStatus();

                var today = new Date();

                if (!contract.active) {
                    if (contract.concluded < today && today < contract.expirationDate) {
                        $scope.displayActive = true;
                    } else {
                        $scope.displayActive = false;
                    }
                } else {
                    $scope.displayActive = false;
                }

                $scope.datepickerOptions = Kitos.Configs.standardKendoDatePickerOptions;

                $scope.procurementPlans = [];
                var currentDate = moment();
                for (var i = 0; i < 40; i++) {
                    var quarter = currentDate.quarter();
                    var year = currentDate.year();
                    var obj = { id: String(i), text: `Q${quarter} | ${year}`, quarter: quarter, year: year };
                    $scope.procurementPlans.push(obj);

                    // add 3 months for next iter
                    currentDate.add(3, 'months');
                }

                var foundPlan: { id } = _.find($scope.procurementPlans, function (plan: { quarter; year; id; }) {
                    return plan.quarter == contract.procurementPlanQuarter && plan.year == contract.procurementPlanYear;
                });
                if (foundPlan) {
                    // plan is found in the list, replace it to get object equality
                    $scope.procurementPlanId = foundPlan;
                } else {
                    // plan is not found, add missing plan to begining of list
                    // if not null
                    if (contract.procurementPlanQuarter != null) {
                        var plan = { id: String($scope.procurementPlans.length), text: contract.procurementPlanQuarter + " | " + contract.procurementPlanYear, quarter: contract.procurementPlanQuarter, year: contract.procurementPlanYear };
                        $scope.procurementPlans.unshift(plan); // add to list
                        $scope.procurementPlanId = plan; // select it
                    }
                }

                $scope.patchDate = (field, value) => {

                    if (Kitos.Helpers.DateValidationHelper.validateDateInput(value, notify, "dato", true)) {
                        var payload = {};
                        if (!value) {
                            payload[field] = null;
                            patch(payload, $scope.autosaveUrl2 + '?organizationId=' + user.currentOrganizationId);
                        } else {
                            const date = Kitos.Helpers.DateStringFormat.fromDanishToEnglishFormat(value);
                            payload[field] = date;
                            patch(payload, $scope.autosaveUrl2 + '?organizationId=' + user.currentOrganizationId);
                        }
                    }
                }

                $scope.saveProcurement = function (id) {
                    if (id === null && contract.procurementPlanQuarter !== null && contract.procurementPlanYear !== null) {
                        updateProcurement(null, null);
                    }
                    else {
                        if (id === null) {
                            return;
                        }

                        var result = _.find($scope.procurementPlans, (plan) => plan.id === id);
                        if (result.quarter === contract.procurementPlanQuarter && result.year === contract.procurementPlanYear) {
                            return;
                        }
                        updateProcurement(result.quarter, result.year);
                    }
                };

                function reloadValidationStatus() {
                    itContractService.getValidationDetails(contract.id).then(newStatus => {
                        $scope.validationStatus = newStatus;
                    });
                }

                $scope.toggleOverride = () => {
                    const newActive = !contract.active;
                    patch({ active: newActive }, $scope.autosaveUrl2 + '?organizationId=' + user.currentOrganizationId).then(updatedContract => {
                        contract.active = updatedContract.active;
                        reloadValidationStatus();
                    });
                }

                $scope.reloadValidationStatus = () => reloadValidationStatus();

                function updateProcurement(procurementPlanQuarter, procurementPlanYear) {
                    contract = $scope.contract;

                    var payload = { procurementPlanQuarter: procurementPlanQuarter, procurementPlanYear: procurementPlanYear };
                    $scope.contract.procurementPlanQuarter = payload.procurementPlanQuarter;
                    $scope.contract.procurementPlanYear = payload.procurementPlanYear;
                    patch(payload, $scope.autoSaveUrl + '?organizationId=' + user.currentOrganizationId);
                }

                function patch(payload, url) {
                    var msg = notify.addInfoMessage("Gemmer...", false);
                    return $http({ method: 'PATCH', url: url, data: payload })
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Feltet er opdateret.");
                            return result.data.response;
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                        });
                }

                if (contract.parentId) {
                    $scope.contract.parent = {
                        id: contract.parentId,
                        text: contract.parentName
                    };
                }

                if (!!contract.supplierId) {
                    $scope.contract.supplier = {
                        id: contract.supplierId,
                        text: contract.supplierName
                    };
                }

                $scope.suppliersSelectOptions = select2LoadingService.loadSelect2WithDataHandler(Kitos.Constants.Organization.BaseApiPath, true, Kitos.Helpers.Select2ApiQueryHelper.getOrganizationQueryParams(100), (item, items) => {
                    items.push({
                        id: item.id,
                        text: item.name ? item.name : 'Unavngiven',
                        cvr: item.cvrNumber
                    });
                }, "q", Kitos.Helpers.Select2OptionsFormatHelper.formatOrganizationWithCvr);

                $scope.checkContractValidity = (field, value) => {
                    var expirationDate = $scope.contract.expirationDate;
                    var concluded = $scope.contract.concluded;
                    var payload = {};


                    if (!value) {
                        payload[field] = null;
                        patch(payload, $scope.autosaveUrl2 + '?organizationId=' + user.currentOrganizationId)
                            .then(_ => reloadValidationStatus());
                    }
                    else if (Kitos.Helpers.DateValidationHelper.validateValidityPeriod(concluded, expirationDate, notify, "Gyldig fra", "Gyldig til")) {
                        const dateString = Kitos.Helpers.DateStringFormat.fromDanishToEnglishFormat(value);
                        payload[field] = dateString;
                        patch(payload, $scope.autosaveUrl2 + '?organizationId=' + user.currentOrganizationId)
                            .then(_ => reloadValidationStatus());
                    }
                }

                function bindCriticalities(contract: any) {

                    const optionMap = Kitos.Helpers.OptionEntityHelper.createDictionaryFromOptionList(criticalityOptions);

                    //If selected state is expired, add it for presentation reasons
                    let existingChoice = null;
                    if (contract.criticalityId !== undefined && contract.criticalityId !== null) {
                        existingChoice = {
                            id: contract.criticalityId,
                            name: `${contract.criticalityName} (udgået)`
                        };

                        if (!optionMap[existingChoice.id]) {
                            optionMap[existingChoice.id] = {
                                text: existingChoice.name,
                                id: existingChoice.id,
                                disabled: true,
                                optionalObjectContext: existingChoice
                            }
                        }
                    }

                    const options = criticalityOptions.map(option => optionMap[option.Id]);

                    $scope.criticality = {
                        selectedElement: existingChoice && optionMap[existingChoice.id],
                        select2Config: select2LoadingService.select2LocalDataNoSearch(() => options, true),
                        elementSelected: (newElement) => {
                            var payload = { criticalityId: newElement ? newElement.id : null };
                            $scope.contract.criticalityId = newElement?.id;
                            patch(payload, $scope.autosaveUrl2 + '?organizationId=' + user.currentOrganizationId);
                        }
                    };
                }

                function bindProcurementInitiated() {
                    const options = new Kitos.Models.ViewModel.Shared.YesNoUndecidedOptions();
                    $scope.procurementInitiated = {
                        selectedElement: options.getById($scope.contract.procurementInitiated),
                        select2Config: select2LoadingService.select2LocalDataNoSearch(() => options.options, false),
                        elementSelected: (newElement) => {
                            if (!!newElement) {
                                $scope.contract.procurementInitiated = newElement.id;
                                var payload = { procurementInitiated: newElement.id };

                                patch(payload, $scope.autoSaveUrl + '?organizationId=' + user.currentOrganizationId);
                            }
                        }
                    }
                }
            }]);
})(angular, app);
