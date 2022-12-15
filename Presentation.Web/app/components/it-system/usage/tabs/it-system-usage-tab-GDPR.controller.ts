((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.GDPR", {
            url: "/GDPR",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-GDPR.view.html",
            controller: "system.GDPR",
            resolve: {
                sensitivePersonalData: ["$http", "$stateParams", ($http, $stateParams) =>
                    $http.get(`odata/GetSensitivePersonalDataByUsageId(id=${$stateParams.id})`)
                        .then(result => result.data.value)
                ],
                registerTypes: [
                    "$http", "$stateParams", ($http, $stateParams) =>
                        $http.get(`odata/GetRegisterTypesByObjectID(id=${$stateParams.id})`)
                            .then(result => result.data.value)
                ]
            }
        });
    }]);

    app.controller("system.GDPR",
        [
            "$scope", "$http", "$state", "$uibModal", "itSystemUsageService", "notify", "registerTypes", "sensitivePersonalData", "user", "uiState",
            ($scope, $http, $state, $uibModal, itSystemUsageService: Kitos.Services.ItSystemUsage.IItSystemUsageService, notify, registerTypes, sensitivePersonalData, user, uiState: Kitos.Models.UICustomization.ICustomizedModuleUI) => {
                //Usage is pulled from it-system-usage.controller.ts. This means we only need one request to DB to have usage available 
                //On all subpages as we can access it from $scope.usage. Same with $scope.usageViewModel.
                var itSystemUsage = $scope.usage;
                $scope.autoSaveUrl = `/api/itsystemusage/${itSystemUsage.id}`;
                $scope.dataOptions = new Kitos.Models.ViewModel.ItSystemUsage.DataOptions().options;
                $scope.riskLevelOptions = new Kitos.Models.ViewModel.ItSystemUsage.RiskLevelOptions().options;
                $scope.hostedAtOptions = Kitos.Models.ViewModel.ItSystemUsage.HostedAtOptions.options;
                $scope.noSearchNoClearSelect2 = { minimumResultsForSearch: -1, allowClear: false };

                $scope.registerTypes = registerTypes;
                $scope.sensitivityLevels = Kitos.Models.ViewModel.ItSystemUsage.SensitiveDataLevelViewModel.levels;
                $scope.sensitivePersonalData = _.orderBy(sensitivePersonalData, "Priority", "desc");
                $scope.personalData = new Kitos.Models.ViewModel.ItSystemUsage.PersonalDataViewModel(itSystemUsage.personalData);

                const blueprint = Kitos.Models.UICustomization.Configs.BluePrints.ItSystemUsageUiCustomizationBluePrint;

                $scope.showPlannedRiskAssessmentDate = uiState.isBluePrintNodeAvailable(blueprint.children.gdpr.children.plannedRiskAssessmentDate);

                $scope.updateDataLevel = (optionId, checked, optionType) => {
                    var msg = notify.addInfoMessage("Arbejder ...", false);
                    if (checked === true) {
                        var data = {
                            ObjectId: itSystemUsage.id,
                            OptionId: optionId,
                            OptionType: optionType,
                            ObjectType: "ITSYSTEMUSAGE"
                        };

                        $http.post(`odata/AttachedOptions?organizationId=${user.currentOrganizationId}`, data, { handleBusy: true })
                            .then(function onSuccess(result) {
                                msg.toSuccessMessage("Feltet er Opdateret.");
                            }, function onError(result) {
                                msg.toErrorMessage("Fejl!");
                            });
                    } else {
                        let optType = 0;
                        switch (optionType) {
                            case "SENSITIVEPERSONALDATA":
                                optType = 1;
                                break;
                            case "REGISTERTYPEDATA":
                                optType = 2;
                                break;
                        }
                        $http.delete(`odata/RemoveOption(id=${optionId}, objectId=${itSystemUsage.id},type=${optType}, entityType=1)`)
                            .then(function onSuccess(result) {
                                msg.toSuccessMessage("Feltet er Opdateret.");
                            }, function onError(result) {
                                msg.toErrorMessage("Fejl!");
                            });
                    }
                }

                $scope.updatePersonalData = (record: Kitos.Models.ViewModel.ItSystemUsage.IPersonalDataModel) => {
                    if (record.checked) {
                        itSystemUsageService.patchPersonalData(itSystemUsage.id, record.value)
                            .then(() => updatePersonalDataSelectionValue(record));
                        return;
                    }
                    itSystemUsageService.removePersonalData(itSystemUsage.id, record.value)
                        .then(() => updatePersonalDataSelectionValue(record));
                }

                function updatePersonalDataSelectionValue(selectedRecord: Kitos.Models.ViewModel.ItSystemUsage.IPersonalDataModel) {
                    if (selectedRecord.checked) {
                        const isOptionWithSameValuePresent = itSystemUsage.personalData.some(x => x === selectedRecord.value);
                        if (isOptionWithSameValuePresent === false) {
                            itSystemUsage.personalData.push(selectedRecord.value);
                        }
                        return;
                    }

                    const selectedOptionIndex = itSystemUsage.personalData.indexOf(x => x === selectedRecord.value);
                    
                    if (selectedOptionIndex.length !== 1) {
                        itSystemUsage.personalData.splice(selectedOptionIndex, 1);
                    }
                }

                $scope.patch = (field, value) => {
                    var payload = {};
                    payload[field] = value;
                    itSystemUsageService.patchSystemUsage(itSystemUsage.id, user.currentOrganizationId, payload)
                        .then(onSuccess => notify.addSuccessMessage("Feltet er opdateret!")
                            , onError => notify.addErrorMessage("Fejl! Feltet kunne ikke opdateres!"));
                }

                $scope.patchDate = (field, value, fieldName) => {
                    var payload = {};
                    if (!value) {
                        payload[field] = null;
                        itSystemUsageService.patchSystemUsage(itSystemUsage.id, user.currentOrganizationId, payload)
                            .then(onSuccess => notify.addSuccessMessage("Feltet er opdateret!")
                                , onError => notify.addErrorMessage("Fejl! Feltet kunne ikke opdateres!"));
                    }
                    else if (Kitos.Helpers.DateValidationHelper.validateDateInput(value, notify, fieldName, true)) {
                        var date = Kitos.Helpers.DateStringFormat.fromDanishToEnglishFormat(value);
                        payload[field] = date;
                        itSystemUsageService.patchSystemUsage(itSystemUsage.id, user.currentOrganizationId, payload)
                            .then(onSuccess => notify.addSuccessMessage("Feltet er opdateret!")
                                , onError => notify.addErrorMessage("Fejl! Feltet kunne ikke opdateres!"));
                    }
                }

                $scope.editLink = field => {
                    $uibModal.open({
                        templateUrl: "app/components/it-system/usage/tabs/it-systemusage-tab-gdpr-editlink-modal.view.html",
                        resolve: {
                            usage: [() => $scope.usage]
                        },
                        controller: [
                            "$scope", "$uibModalInstance", "usage",
                            ($scope, $uibModalInstance, usage) => {

                                $scope.linkName = usage[field + "Name"];
                                $scope.Url = usage[field];

                                $scope.ok = () => {
                                    var payload = {};
                                    payload[field + "Name"] = $scope.linkName;
                                    payload[field] = $scope.Url;

                                    var msg = notify.addInfoMessage("Gemmer...", false);
                                    itSystemUsageService
                                        .patchSystemUsage(usage.id, user.currentOrganizationId, payload)
                                        .then(onSuccess => {
                                            msg.toSuccessMessage("Feltet er opdateret.");
                                            $state.reload();
                                        }, onError => {
                                            msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                        });
                                    $uibModalInstance.close();
                                };

                                $scope.cancel = () => {
                                    $uibModalInstance.dismiss("cancel");
                                };
                            }
                        ],
                    });
                }

                $scope.toggleSelection = data => {
                    var idx = $scope.selection.indexOf(data);
                    // Is currently selected
                    if (idx > -1) {
                        $scope.selection.splice(idx, 1);
                    }

                    // Is newly selected
                    else {
                        $scope.selection.push(data);
                    }
                };

                $scope.dataLevelChange = (dataLevel: number) => {
                    switch (dataLevel) {
                        case Kitos.Models.ViewModel.ItSystemUsage.SensitiveDataLevelViewModel.levels.none.value:
                            updateDataLevels(dataLevel, $scope.usageViewModel.noDataSelected);
                            break;
                        case Kitos.Models.ViewModel.ItSystemUsage.SensitiveDataLevelViewModel.levels.personal.value:
                            updateDataLevels(dataLevel, $scope.usageViewModel.personalDataSelected);
                            break;
                        case Kitos.Models.ViewModel.ItSystemUsage.SensitiveDataLevelViewModel.levels.sensitive.value:
                            updateDataLevels(dataLevel, $scope.usageViewModel.sensitiveDataSelected);
                            break;
                        case Kitos.Models.ViewModel.ItSystemUsage.SensitiveDataLevelViewModel.levels.legal.value:
                            updateDataLevels(dataLevel, $scope.usageViewModel.legalDataSelected);
                            break;
                        default:
                            break;
                    }
                };

                function updateDataLevels(dataLevel: number, selected: boolean) {
                    if (selected) {
                        itSystemUsageService.addDataLevel(itSystemUsage.id, dataLevel)
                            .then(onSuccess => notify.addSuccessMessage("Feltet er opdateret!"),
                                onError => notify.addErrorMessage("Kunne ikke opdatere feltet!"));
                    }
                    else {
                        itSystemUsageService.removeDataLevel(itSystemUsage.id, dataLevel)
                            .then(onSuccess => {
                                notify.addSuccessMessage("Feltet er opdateret!");
                                if (dataLevel === Kitos.Models.ViewModel.ItSystemUsage.SensitiveDataLevelViewModel.levels.personal.value) {
                                    itSystemUsageService.getItSystemUsage(itSystemUsage.id)
                                        .then(newUsage => {
                                                $scope.usage.personalData = newUsage.personalData;
                                            $scope.personalData = new Kitos.Models.ViewModel.ItSystemUsage.PersonalDataViewModel($scope.usage.personalData);
                                            },
                                            error => console.log("Failed to load a new it system usage while updating the PersonalData values"));
                                }
                            },
                            onError => notify.addErrorMessage("Kunne ikke opdatere feltet!"));
                    }
                }

                $scope.datepickerOptions = Kitos.Configs.standardKendoDatePickerOptions;
            }]);

})(angular, app);
