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
            "$scope", "$http", "$state", "$uibModal", "$stateParams", "$timeout", "itSystemUsageService", "moment", "notify", "registerTypes", "sensitivePersonalData", "user", "select2LoadingService",
            ($scope, $http, $state, $uibModal, $stateParams, $timeout, itSystemUsageService, moment, notify, registerTypes, sensitivePersonalData, user, select2LoadingService) => {
                //Usage is pulled from it-system-usage.controller.ts. This means we only need one request to DB to have usage available 
                //On all subpages as we can access it from $scope.usage. Same with $scope.usageViewModel.
                var itSystemUsage = $scope.usage;
                $scope.autoSaveUrl = `/api/itsystemusage/${itSystemUsage.id}`;
                $scope.dataOptions = new Kitos.Models.ViewModel.ItSystemUsage.DataOptions().options;
                $scope.riskLevelOptions = new Kitos.Models.ViewModel.ItSystemUsage.RiskLevelOptions().options;
                $scope.hostedAtOptions = new Kitos.Models.ViewModel.ItSystemUsage.HostedAtOptions().options;
                $scope.noSearchNoClearSelect2 = { minimumResultsForSearch: -1, allowClear: false };

                $scope.registerTypes = registerTypes;
                $scope.sensitivityLevels = Kitos.Models.ViewModel.ItSystemUsage.SensitiveDataLevelViewModel.levels;
                $scope.sensitivePersonalData = _.orderBy(sensitivePersonalData, "Priority", "desc");
                $scope.contracts = itSystemUsage.contracts.filter(x => (x.contractTypeName === "Databehandleraftale"));
                $scope.filterDataProcessor = $scope.contracts.length > 0;

                $scope.dataWorkerSelectOptions = select2LoadingService.loadSelect2WithDataHandler(
                    "api/organization",
                    true,
                    ['take=25',`orgId=${user.currentOrganizationId}`],
                    (item,
                        items) => {
                        items.push({
                            id: item.id,
                            text: item.name,
                            cvr: item.cvr
                        });
                    },
                    "q",
                    formatSupplier);

                function formatSupplier(supplier) {
                    var result = `<div>${supplier.text}</div>`;
                    if (supplier.cvr) {
                        result += `<div class="small">${supplier.cvr}</div>`;
                    }
                    return result;
                }

                $scope.updateDataLevel = (optionId, checked, optionType) => {
                    var msg = notify.addInfoMessage("Arbejder ...", false);
                    if (checked === true) {
                        var data = {
                            ObjectId: itSystemUsage.id,
                            OptionId: optionId,
                            OptionType: optionType,
                            ObjectType: "ITSYSTEMUSAGE"
                        };

                        $http.post("odata/AttachedOptions/", data, { handleBusy: true }).success(result => {
                            msg.toSuccessMessage("Feltet er Opdateret.");
                        }).error(() => {
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
                        $http.delete(`odata/RemoveOption(id=${optionId}, objectId=${itSystemUsage.id},type=${optType}, entityType=1)`).success(() => {
                            msg.toSuccessMessage("Feltet er Opdateret.");
                        }).error(() => {
                            msg.toErrorMessage("Fejl!");
                        });
                    }
                }

                $scope.patch = (field, value) => {
                    var payload = {};
                    payload[field] = value;
                    itSystemUsageService.patchSystemUsage(itSystemUsage.id, user.currentOrganizationId, payload)
                        .then(onSuccess => notify.addSuccessMessage("Feltet er opdateret!")
                            , onError => notify.addErrorMessage("Fejl! Feltet kunne ikke opdateres!"));
                }

                $scope.patchDate = (field, value) => {
                    var date = moment(value, "DD-MM-YYYY");
                    var payload = {};
                    if (value === "" || value == undefined) {
                        payload[field] = null;
                        itSystemUsageService.patchSystemUsage(itSystemUsage.id, user.currentOrganizationId, payload)
                            .then(onSuccess => notify.addSuccessMessage("Feltet er opdateret!")
                            , onError => notify.addErrorMessage("Fejl! Feltet kunne ikke opdateres!"));
                    } else if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                        notify.addErrorMessage("Den indtastede dato er ugyldig.");
                    } else {
                        date = date.format("YYYY-MM-DD");
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

                $scope.saveDataworker = () => {
                    var data = {
                        ItSystemUsageId: $scope.usage.id,
                        DataWorkerId: $scope.selectedDataWorker.id
                    }
                    $http.post("api/UsageDataworker/", data)
                        .success(() => {
                            notify.addSuccessMessage("Databehandleren er tilknyttet.");
                            reload();
                        })
                        .error(() => {
                            notify.addErrorMessage("Fejl! Kunne ikke tilknytte databehandleren!");
                        });
                };

                $scope.deleteDataworker = dataworkerId => {
                    $http.delete(`api/UsageDataWorker/${dataworkerId}?organizationid=${$scope.usage.organizationId}`)
                        .success(() => {
                            notify.addSuccessMessage("Databehandlerens tilknyttning er fjernet.");
                            reload();
                        })
                        .error(() => {
                            notify.addErrorMessage("Fejl! Kunne ikke fjerne databehandlerens tilknyttning!");
                        });
                };

                function reload() {
                    return $state.transitionTo($state.current, $stateParams, {
                        reload: true
                    }).then(() => {
                        $scope.hideContent = true;
                        return $timeout(() => $scope.hideContent = false, 1);
                    });
                }
                

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
                            .then(onSuccess => notify.addSuccessMessage("Feltet er opdateret!"),
                                onError => notify.addErrorMessage("Kunne ikke opdatere feltet!"));
                    }
                }

                $scope.datepickerOptions = {
                    format: "dd-MM-yyyy"
                };
            }]);

})(angular, app);
