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
                sensitivePersonalData: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("odata/GetSensitivePersonalDataByUsageId(id=" + $stateParams.id + ")")
                        .then(function (result) {
                            return result.data.value;
                        });
                }],
                registerTypes: [
                    '$http', '$stateParams', ($http, $stateParams) =>
                        $http.get(`odata/GetRegisterTypesByObjectID(id=${$stateParams.id})`)
                            .then(result => result.data.value)
                ],
                dataResponsible: ['$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                    return $http.get('api/dataResponsible/' + user.currentOrganizationId)
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller("system.GDPR",
        [
            "$scope", "$http", "$state", "$uibModal", "$stateParams", "$timeout", "itSystemUsage", "itSystemUsageService", "systemUsage", "moment", "notify", "registerTypes", "sensitivePersonalData", "user", "dataResponsible",
            ($scope, $http, $state, $uibModal, $stateParams, $timeout, itSystemUsage, itSystemUsageService, systemUsage, moment, notify, registerTypes, sensitivePersonalData, user, dataResponsible) => {

                $scope.usage = itSystemUsage;
                $scope.usageViewModel = new Kitos.Models.ViewModel.ItSystemUsage.SystemUsageViewModel($scope.usage);
                $scope.autoSaveUrl = `/api/itsystemusage/${itSystemUsage.id}`;
                $scope.dataOptions = new Kitos.Models.ViewModel.ItSystemUsage.DataOptions().options;
                $scope.riskLevelOptions = new Kitos.Models.ViewModel.ItSystemUsage.RiskLevelOptions().options;
                $scope.sensitiveDataLevel = Kitos.Models.ViewModel.ItSystemUsage.SensitiveDataLevel;
                $scope.registerTypes = registerTypes;
                $scope.usageId = $stateParams.id;
                $scope.systemUsage = systemUsage;
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
                if (!systemUsage.dataProcessor) {
                    $scope.systemUsage.dataProcessor = dataResponsible.name;
                }
                $scope.updateUrl = '/api/itsystemusage/' + $scope.usage.id;
                $scope.dataWorkerSelectOptions = selectLazyLoading('api/organization', false, ['public=true', 'orgId=' + user.currentOrganizationId]);

                $scope.systemUsage.LinkToDirectoryUrl = encodeURI(systemUsage.LinkToDirectoryUrl);

                $scope.updateDataLevel = function (OptionId, Checked, optionType, entitytype) {

                    var msg = notify.addInfoMessage("Arbejder ...", false);

                    if (Checked == true) {

                        var data = {
                            ObjectId: itSystemUsage.id,
                            OptionId: OptionId,
                            OptionType: optionType,
                            ObjectType: 'ITSYSTEMUSAGE'
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
                        $http.delete("odata/RemoveOption(id=" + OptionId + ", objectId=" + itSystemUsage.id + ",type=" + OptType + ", entityType=1)").success(() => {
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
                        $scope.ArchivedDate = systemUsage.ArchivedDate;
                    } else {
                        date = date.format("YYYY-MM-DD");
                        var payload = {};
                        payload[field] = date;
                        itSystemUsageService.patchSystem($scope.usageId, payload);
                        $scope.ArchivedDate = date;
                    }
                }

                $scope.editLink = function (field) {
                    $uibModal.open({
                        templateUrl: 'app/components/it-system/usage/tabs/it-systemusage-tab-gdpr-editlink-modal.view.html',
                        controller: [
                            '$scope', '$state', '$uibModalInstance', 'usage',
                            function ($scope, $state, $uibModalInstance, usage) {

                                $scope.usage = usage;

                                switch (field) {
                                    case 'datahandlerSupervisionDocumentationUrl':
                                        $scope.linkName = $scope.usage.datahandlerSupervisionDocumentationUrlName;
                                        $scope.Url = $scope.usage.datahandlerSupervisionDocumentationUrl;
                                        break;
                                    case 'technicalSupervisionDocumentationUrl':
                                        $scope.linkName = $scope.usage.TechnicalSupervisionDocumentationUrlName;
                                        $scope.Url = $scope.usage.TechnicalSupervisionDocumentationUrl;
                                        break;
                                    case 'UserSupervisionDocumentationUrl':
                                        $scope.linkName = $scope.usage.UserSupervisionDocumentationUrlName;
                                        $scope.Url = $scope.usage.UserSupervisionDocumentationUrl;
                                        break;
                                    case 'RiskSupervisionDocumentationUrl':
                                        $scope.linkName = $scope.usage.RiskSupervisionDocumentationUrlName;
                                        $scope.Url = $scope.usage.RiskSupervisionDocumentationUrl;
                                        break;
                                    case 'DPIASupervisionDocumentationUrl':
                                        $scope.linkName = $scope.usage.DPIASupervisionDocumentationUrlName;
                                        $scope.Url = $scope.usage.DPIASupervisionDocumentationUrl;
                                        break;
                                    case 'LinkToDirectoryUrl':
                                        $scope.linkName = $scope.usage.LinkToDirectoryUrlName;
                                        $scope.Url = $scope.usage.LinkToDirectoryUrl;
                                        break;
                                }
                                $scope.ok = function () {

                                    var payload = {
                                        Id: $scope.usage.Id,

                                        datahandlerSupervisionDocumentationUrlName: $scope.usage
                                            .DatahandlerSupervisionDocumentationUrlName,
                                        datahandlerSupervisionDocumentationUrl: $scope.usage
                                            .DatahandlerSupervisionDocumentationUrl,

                                        technicalSupervisionDocumentationUrlName: $scope.usage
                                            .TechnicalSupervisionDocumentationUrlName,
                                        technicalSupervisionDocumentationUrl: $scope.usage
                                            .TechnicalSupervisionDocumentationUrl,

                                        userSupervisionDocumentationUrlName: $scope.usage
                                            .UserSupervisionDocumentationUrlName,
                                        userSupervisionDocumentationUrl: $scope.usage.UserSupervisionDocumentationUrl,

                                        riskSupervisionDocumentationUrlName: $scope.usage
                                            .RiskSupervisionDocumentationUrlName,
                                        riskSupervisionDocumentationUrl: $scope.usage.RiskSupervisionDocumentationUrl,

                                        DPIASupervisionDocumentationUrlName: $scope.usage
                                            .DPIASupervisionDocumentationUrlName,
                                        DPIASupervisionDocumentationUrl: $scope.usage.DPIASupervisionDocumentationUrl,

                                        LinkToDirectoryUrlName: $scope.usage.LinkToDirectoryUrlName,
                                        LinkToDirectoryUrl: $scope.usage.LinkToDirectoryUrl
                                    }

                                    switch (field) {
                                        case 'datahandlerSupervisionDocumentationUrl':
                                            payload.datahandlerSupervisionDocumentationUrlName = $scope.linkName;
                                            payload.datahandlerSupervisionDocumentationUrl = $scope.Url;
                                            break;
                                        case 'technicalSupervisionDocumentationUrl':
                                            payload.technicalSupervisionDocumentationUrlName = $scope.linkName;
                                            payload.technicalSupervisionDocumentationUrl = $scope.Url;
                                            break;
                                        case 'UserSupervisionDocumentationUrl':
                                            payload.userSupervisionDocumentationUrlName = $scope.linkName;
                                            payload.userSupervisionDocumentationUrl = $scope.Url;
                                            break;
                                        case 'RiskSupervisionDocumentationUrl':
                                            payload.riskSupervisionDocumentationUrlName = $scope.linkName;
                                            payload.riskSupervisionDocumentationUrl = $scope.Url;
                                            break;
                                        case 'DPIASupervisionDocumentationUrl':
                                            payload.DPIASupervisionDocumentationUrlName = $scope.linkName;
                                            payload.DPIASupervisionDocumentationUrl = $scope.Url;
                                            break;
                                        case 'LinkToDirectoryUrl':
                                            payload.LinkToDirectoryUrlName = $scope.linkName;
                                            payload.LinkToDirectoryUrl = $scope.Url;
                                            break;
                                    }

                                    var msg = notify.addInfoMessage("Gemmer...", false);

                                    $http({
                                        method: 'PATCH',
                                        url: 'api/itsystemusage/' +
                                            $scope.usage.Id +
                                            '?organizationId=' +
                                            user.currentOrganizationId,
                                        data: payload
                                    })
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
                            }
                        ],
                        resolve: {
                            usage: [function () { return $scope.systemUsage; }]
                        }
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

                $scope.delete = function (dataworkerId) {
                    $http.delete("api/UsageDataWorker/" + dataworkerId + "?organizationid=" + $scope.usage.organizationId)
                        .success(function () {
                            notify.addSuccessMessage("Databehandlerens tilknyttning er fjernet.");
                            reload();
                        })
                        .error(function () {
                            notify.addErrorMessage("Fejl! Kunne ikke fjerne databehandlerens tilknyttning!");
                        });
                };
                function selectLazyLoading(url, excludeSelf, paramAry) {
                    return {
                        minimumInputLength: 1,
                        allowClear: true,
                        placeholder: ' ',
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
                                    if (excludeSelf && obj.id == itSystemUsage.id)
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

                function reload() {
                    return $state.transitionTo($state.current, $stateParams, {
                        reload: true
                    }).then(function () {
                        $scope.hideContent = true;
                        return $timeout(function () {
                            return $scope.hideContent = false;
                        }, 1);
                    });
                }
                $scope.save = function () {

                    var data = {
                        ItSystemUsageId: $scope.usage.id,
                        DataWorkerId: $scope.selectedDataWorker.id
                    }

                    $http.post("api/UsageDataworker/", data)
                        .success(function () {
                            notify.addSuccessMessage("Databehandleren er tilknyttet.");
                            reload();
                        })
                        .error(function () {
                            notify.addErrorMessage("Fejl! Kunne ikke tilknytte databehandleren!");
                        });
                };

                $scope.dataLevelChange = (dataLevel: Kitos.Models.ViewModel.ItSystemUsage.SensitiveDataLevel) => {
                    switch (dataLevel) {
                        case Kitos.Models.ViewModel.ItSystemUsage.SensitiveDataLevel.NONE:
                            updateDataLevels(dataLevel, $scope.usageViewModel.personalNoDataSelected);
                            break;
                        case Kitos.Models.ViewModel.ItSystemUsage.SensitiveDataLevel.PERSONALDATA:
                            updateDataLevels(dataLevel, $scope.usageViewModel.personalRegularDataSelected);
                            break;
                        case Kitos.Models.ViewModel.ItSystemUsage.SensitiveDataLevel.SENSITIVEDATA:
                            updateDataLevels(dataLevel, $scope.usageViewModel.personalSensitiveDataSelected);
                            break;
                        case Kitos.Models.ViewModel.ItSystemUsage.SensitiveDataLevel.LEGALDATA:
                            updateDataLevels(dataLevel, $scope.usageViewModel.personalLegalDataSelected);
                            break;
                        default:
                            break;
                    }
                };

                function updateDataLevels(dataLevel: Kitos.Models.ViewModel.ItSystemUsage.SensitiveDataLevel, boolValue: boolean) {
                    if (boolValue) {
                        $http.patch(`api/v1/itsystemusage/${itSystemUsage.id}/sensitivityLevel/add`, dataLevel)
                            .then(onSuccess => notify.addSuccessMessage("Feltet er opdateret!"),
                                onError => notify.addErrorMessage("Kunne ikke opdatere feltet!"));
                    }
                    else {
                        $http.patch(`api/v1/itsystemusage/${itSystemUsage.id}/sensitivityLevel/remove`, dataLevel)
                            .then(onSuccess => notify.addSuccessMessage("Feltet er opdateret!"),
                                onError => notify.addErrorMessage("Kunne ikke opdatere feltet!"));
                    }
                }

                $scope.datepickerOptions = {
                    format: "dd-MM-yyyy"
                };
            }]);

})(angular, app);
