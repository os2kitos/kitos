((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.GDPR", {
            url: "/GDPR",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-GDPR.view.html",
            controller: "system.GDPR",
            resolve: {
                systemUsage: [
                    "$http", "$stateParams", ($http, $stateParams) =>
                        $http.get(`odata/itSystemUsages(${$stateParams.id})`)
                            .then(result => result.data)
                ],
                sensitivePersonalData: ["$http", "$stateParams", ($http, $stateParams) =>
                    $http.get(`odata/GetSensitivePersonalDataByUsageId(id=${$stateParams.id})`)
                        .then(result => result.data.value)
                ],
                registerTypes: [
                    "$http", "$stateParams", ($http, $stateParams) =>
                        $http.get(`odata/GetRegisterTypesByObjectID(id=${$stateParams.id})`)
                            .then(result => result.data.value)
                ],
                dataResponsible: ["$http", "user", ($http, user) =>
                    $http.get(`api/dataResponsible/${user.currentOrganizationId}`)
                        .then(result => result.data.response)
                ]
            }
        });
    }]);

    app.controller("system.GDPR",
        [
            "$scope", "$http", "$state", "$uibModal", "$stateParams", "$timeout", "itSystemUsageService", "systemUsage", "moment", "notify", "registerTypes", "sensitivePersonalData", "user", "dataResponsible", "GDPRService",
            ($scope, $http, $state, $uibModal, $stateParams, $timeout, itSystemUsageService, systemUsage, moment, notify, registerTypes, sensitivePersonalData, user, dataResponsible, GDPRService) => {
                //Usage is pulled from it-system-usage.controller.ts. This means we only need one request to DB to have usage available 
                //On all subpages as we can access it from $scope.usage. Same with $scope.usageViewModel.
                var itSystemUsage = $scope.usage;
                $scope.autoSaveUrl = `/api/itsystemusage/${itSystemUsage.id}`;
                $scope.dataOptions = new Kitos.Models.ViewModel.ItSystemUsage.DataOptions().options;
                $scope.riskLevelOptions = new Kitos.Models.ViewModel.ItSystemUsage.RiskLevelOptions().options;

                $scope.registerTypes = registerTypes;
                $scope.usageId = $stateParams.id;
                $scope.systemUsage = systemUsage;
                $scope.sensitivityLevels = Kitos.Models.ViewModel.ItSystemUsage.SensitiveDataLevelViewModel.levels;
                $scope.sensitivePersonalData = _.orderBy(sensitivePersonalData, "Priority", "desc");
                $scope.contracts = itSystemUsage.contracts.filter(x => (x.contractTypeName === "Databehandleraftale" || x.agreementElements.some(y => y.name === "Databehandleraftale")));
                $scope.filterDataProcessor = $scope.contracts.length > 0;

                //inherit from parent if general purpose is empty
                $scope.generalPurpose = itSystemUsage.generalPurpose;
                if (!$scope.generalPurpose) {
                    $scope.generalPurpose = itSystemUsage.itSystem.generalPurpose;
                }
                //inherit from parent
                $scope.systemUsage.LinkToDirectoryUrl = itSystemUsage.linkToDirectoryUrl;
                if (!$scope.systemUsage.LinkToDirectoryUrl) {
                    $scope.systemUsage.LinkToDirectoryUrl = itSystemUsage.itSystem.linkToDirectoryAdminUrl;
                }
                //inherit from parent
                $scope.systemUsage.LinkToDirectoryUrlName = itSystemUsage.linkToDirectoryUrlName;
                if (!$scope.systemUsage.LinkToDirectoryUrlName) {
                    $scope.systemUsage.LinkToDirectoryUrlName = itSystemUsage.itSystem.linkToDirectoryAdminUrlName;
                }
                //inherit from parent
                if (!$scope.systemUsage.dataProcessor) {
                    $scope.systemUsage.dataProcessor = dataResponsible.name;
                }
                $scope.updateUrl = `/api/itsystemusage/${$scope.usage.id}`;
                $scope.dataWorkerSelectOptions = selectLazyLoading("api/organization", false, ["public=true", `orgId=${user.currentOrganizationId}`]);

                $scope.systemUsage.LinkToDirectoryUrl = encodeURI($scope.systemUsage.LinkToDirectoryUrl);

                $scope.updateDataLevel = (OptionId, Checked, optionType, entitytype) => {

                    var msg = notify.addInfoMessage("Arbejder ...", false);

                    if (Checked == true) {

                        var data = {
                            ObjectId: itSystemUsage.id,
                            OptionId: OptionId,
                            OptionType: optionType,
                            ObjectType: "ITSYSTEMUSAGE"
                        };

                        $http.post("odata/AttachedOptions/", data, { handleBusy: true }).success(result => {
                            msg.toSuccessMessage("Feltet er Opdateret.");
                        }).error(() => {
                            msg.toErrorMessage("Fejl!");
                        });

                    } else {
                        let OptType = 0;
                        switch (optionType) {
                            case "SENSITIVEPERSONALDATA":
                                OptType = 1;
                                break;
                            case "REGISTERTYPEDATA":
                                OptType = 2;
                                break;
                        }
                        $http.delete(`odata/RemoveOption(id=${OptionId}, objectId=${itSystemUsage.id},type=${OptType}, entityType=1)`).success(() => {
                            msg.toSuccessMessage("Feltet er Opdateret.");
                        }).error(() => {
                            msg.toErrorMessage("Fejl!");
                        });
                    }
                }

                $scope.patch = (field, value) => {
                    var payload = {};
                    payload[field] = value;
                    itSystemUsageService.patchSystem($scope.usageId, payload);
                }

                $scope.patchDate = (field, value) => {
                    var date = moment(value, "DD-MM-YYYY");
                    if (value === "" || value == undefined) {
                        var payload = {};
                        payload[field] = null;
                        itSystemUsageService.patchSystem($scope.usageId, payload);
                    } else if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                        notify.addErrorMessage("Den indtastede dato er ugyldig.");
                        $scope.ArchivedDate = $scope.systemUsage.ArchivedDate;
                    } else {
                        date = date.format("YYYY-MM-DD");
                        var payload = {};
                        payload[field] = date;
                        itSystemUsageService.patchSystem($scope.usageId, payload);
                        $scope.ArchivedDate = date;
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

                                //Ready old data in input fields.
                                switch (field) {
                                    case "datahandlerSupervisionDocumentationUrl":
                                        $scope.linkName = usage.datahandlerSupervisionDocumentationUrlName;
                                        $scope.Url = usage.datahandlerSupervisionDocumentationUrl;
                                        break;
                                    case "technicalSupervisionDocumentationUrl":
                                        $scope.linkName = usage.technicalSupervisionDocumentationUrlName;
                                        $scope.Url = usage.technicalSupervisionDocumentationUrl;
                                        break;
                                    case "userSupervisionDocumentationUrl":
                                        $scope.linkName = usage.userSupervisionDocumentationUrlName;
                                        $scope.Url = usage.userSupervisionDocumentationUrl;
                                        break;
                                    case "riskSupervisionDocumentationUrl":
                                        $scope.linkName = usage.riskSupervisionDocumentationUrlName;
                                        $scope.Url = usage.riskSupervisionDocumentationUrl;
                                        break;
                                    case "dpiaSupervisionDocumentationUrl":
                                        $scope.linkName = usage.dpiaSupervisionDocumentationUrlName;
                                        $scope.Url = usage.dpiaSupervisionDocumentationUrl;
                                        break;
                                    case "linkToDirectoryUrl":
                                        $scope.linkName = usage.linkToDirectoryUrlName;
                                        $scope.Url = usage.linkToDirectoryUrl;
                                        break;
                                }

                                $scope.ok = () => {
                                    var linkName = $scope.linkName;
                                    var url = $scope.Url;
                                    var payload: any = {};

                                    switch (field) {
                                        case "datahandlerSupervisionDocumentationUrl":
                                            payload.datahandlerSupervisionDocumentationUrlName = linkName;
                                            payload.datahandlerSupervisionDocumentationUrl = url;
                                            break;
                                        case "technicalSupervisionDocumentationUrl":
                                            payload.technicalSupervisionDocumentationUrlName = linkName;
                                            payload.technicalSupervisionDocumentationUrl = url;
                                            break;
                                        case "userSupervisionDocumentationUrl":
                                            payload.userSupervisionDocumentationUrlName = linkName;
                                            payload.userSupervisionDocumentationUrl = url;
                                            break;
                                        case "riskSupervisionDocumentationUrl":
                                            payload.riskSupervisionDocumentationUrlName = linkName;
                                            payload.riskSupervisionDocumentationUrl = url;
                                            break;
                                        case "dpiaSupervisionDocumentationUrl":
                                            payload.dpiaSupervisionDocumentationUrlName = linkName;
                                            payload.dpiaSupervisionDocumentationUrl = url;
                                            break;
                                        case "linkToDirectoryUrl":
                                            payload.linkToDirectoryUrlName = linkName;
                                            payload.linkToDirectoryUrl = url;
                                            break;
                                    }

                                    var msg = notify.addInfoMessage("Gemmer...", false);

                                    $http({
                                        method: "PATCH",
                                        url: `api/itsystemusage/${usage.id}?organizationId=${user.currentOrganizationId}`,
                                        data: payload
                                    })
                                        .success(() => {
                                            msg.toSuccessMessage("Feltet er opdateret.");
                                            $state.reload();
                                        })
                                        .error(() => {
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

                $scope.delete = dataworkerId => {
                    $http.delete(`api/UsageDataWorker/${dataworkerId}?organizationid=${$scope.usage.organizationId}`)
                        .success(() => {
                            notify.addSuccessMessage("Databehandlerens tilknyttning er fjernet.");
                            reload();
                        })
                        .error(() => {
                            notify.addErrorMessage("Fejl! Kunne ikke fjerne databehandlerens tilknyttning!");
                        });
                };

                function selectLazyLoading(url: any, excludeSelf: any, paramAry: any);
                function selectLazyLoading(url, excludeSelf, paramAry) {
                    return {
                        minimumInputLength: 1,
                        allowClear: true,
                        placeholder: " ",
                        initSelection(elem, callback) {
                        },
                        ajax: {
                            data(term, page) {
                                return { query: term };
                            },
                            quietMillis: 500,
                            transport(queryParams) {
                                var extraParams = paramAry ? `&${paramAry.join("&")}` : "";
                                var res = $http.get(url + "?q=" + queryParams.data.query + extraParams).then(queryParams.success);
                                res.abort = () => null;

                                return res;
                            },

                            results(data, page) {
                                var results = [];

                                _.each(data.data.response, (obj: { id; name; cvr; }) => {
                                    if (excludeSelf && obj.id == itSystemUsage.id)
                                        return; // don't add self to result

                                    results.push({
                                        id: obj.id,
                                        text: obj.name ? obj.name : "Unavngiven",
                                        cvr: obj.cvr
                                    });
                                });

                                return { results: results };
                            }
                        }
                    };
                }

                function reload() {
                    return $state.transitionTo($state.current, $stateParams, {
                        reload: true
                    }).then(() => {
                        $scope.hideContent = true;
                        return $timeout(() => $scope.hideContent = false, 1);
                    });
                }
                $scope.save = () => {

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
                        GDPRService.addDataLevel(itSystemUsage.id, dataLevel)
                            .then(onSuccess => notify.addSuccessMessage("Feltet er opdateret!"),
                                onError => notify.addErrorMessage("Kunne ikke opdatere feltet!"));
                    }
                    else {
                        GDPRService.removeDataLevel(itSystemUsage.id, dataLevel)
                            .then(onSuccess => notify.addSuccessMessage("Feltet er opdateret!"),
                                onError => notify.addErrorMessage("Kunne ikke opdatere feltet!"));
                    }
                }

                $scope.datepickerOptions = {
                    format: "dd-MM-yyyy"
                };
            }]);

})(angular, app);
