((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.GDPR", {
            url: "/GDPR",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-GDPR.view.html",
            controller: "system.GDPR",
            resolve: {
                systemCategories: [
                    "$http", $http => $http.get("odata/LocalItSystemCategories?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                    .then(result => result.data.value)
                ],
                systemUsage: [
                    "$http", "$stateParams", ($http, $stateParams) =>
                    $http.get(`odata/itSystemUsages(${$stateParams.id})`)
                    .then(result => result.data)
                ],
                regularSensitiveData: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("odata/GetRegularPersonalDataByObjectID(id=" + $stateParams.id + ", entitytype='ITSYSTEMUSAGE')")
                        .then(function (result) {
                            return result.data.value;
                        });
                }],
                sensitivePersonalData: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("odata/GetSensitivePersonalDataByObjectID(id=" + $stateParams.id + ", entitytype='ITSYSTEMUSAGE')")
                        .then(function (result) {
                            return result.data.value;
                        });
                }],
                registerTypes: [
                    '$http', '$stateParams', ($http, $stateParams) =>
                    $http.get(`odata/GetRegisterTypesByObjectID(id=${$stateParams.id})`)
                    .then(result => result.data.value)
                ]
            }
        });
    }]);

    app.controller("system.GDPR",
        [
            "$scope", "$http", "$state", "$uibModal", "$stateParams", "$timeout", "itSystemUsage", "itSystemUsageService", "systemUsage", "systemCategories", "moment", "notify", "registerTypes", "regularSensitiveData", "sensitivePersonalData","user",
            ($scope, $http, $state, $uibModal, $stateParams, $timeout, itSystemUsage, itSystemUsageService, systemUsage, systemCategories, moment, notify, registerTypes, regularSensitiveData, sensitivePersonalData,user) => {

            $scope.usage = itSystemUsage;
            $scope.registerTypes = registerTypes;
            $scope.usageId = $stateParams.id;
            $scope.systemUsage = systemUsage;
            $scope.systemCategories = systemCategories;
            $scope.regularSensitiveData = regularSensitiveData;
            $scope.sensitivePersonalData = sensitivePersonalData;
            $scope.contracts = itSystemUsage.contracts.filter(x => (x.contractTypeName === "Databehandleraftale" || x.agreementElements.some(y => y.name === "Databehandleraftale")));
            $scope.filterDataProcessor = $scope.contracts.length > 0;

                //inherit from parent if general purpose is empty
            $scope.generalPurpose = $scope.usage.generalPurpose;

            if (!$scope.generalPurpose) {
                $scope.generalPurpose = $scope.usage.itSystem.generalPurpose;
            }

            $scope.updateUrl = '/api/itsystemusage/' + $scope.usage.id;
            $scope.dataWorkerSelectOptions = selectLazyLoading('api/organization', false, ['public=true', 'orgId=' + user.currentOrganizationId]);



            $scope.updateDataLevel = function (OptionId, Checked, optionType, entitytype) {

                var msg = notify.addInfoMessage("Arbejder ...", false);

                if (Checked == true) {

                    var data = {
                        ObjectId: itSystemUsage.id,
                        OptionId: OptionId,
                        OptionType: optionType,
                        ObjectType: 'ITSYSTEMUSAGE'
                    };

                    $http.post("Odata/AttachedOptions/", data, { handleBusy: true }).success(result => {
                        msg.toSuccessMessage("Feltet er Opdateret.");
                    }).error(() => {
                        msg.toErrorMessage("Fejl!");
                    });

                } else {
                    $http.delete("Odata/RemoveOption(id=" + OptionId + ", objectId=" + itSystemUsage.id + ",type='" + optionType + "', entityType='ITSYSTEMUSAGE')").success(() => {
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
                var date = moment(moment(value, "DD-MM-YYYY", true).format());

                if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
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
                    controller: ['$scope', '$state', '$uibModalInstance', 'usage', function ($scope, $state, $uibModalInstance, usage) {

                        $scope.usage = usage;

                        switch (field) {
                            case 'datahandlerSupervisionDocumentationUrl':
                                $scope.Url = $scope.usage.datahandlerSupervisionDocumentationUrl;
                                break;
                            case 'technicalSupervisionDocumentationUrl':
                                $scope.Url = $scope.usage.TechnicalSupervisionDocumentationUrl;
                                break;
                            case 'UserSupervisionDocumentationUrl':
                                $scope.Url = $scope.usage.UserSupervisionDocumentationUrl;
                                break;
                            case 'RiskSupervisionDocumentationUrl':
                                $scope.Url = $scope.usage.RiskSupervisionDocumentationUrl;
                                break;
                            case 'DataHearingSupervisionDocumentationUrl':
                                $scope.Url = $scope.usage.DataHearingSupervisionDocumentationUrl;
                                break;
                            case 'DPIASupervisionDocumentationUrl':
                                $scope.Url = $scope.usage.DPIASupervisionDocumentationUrl;
                                break;
                        }


                        $scope.ok = function () {

                            var payload = {
                                Id: $scope.usage.Id,
                                datahandlerSupervisionDocumentationUrl: $scope.usage.DatahandlerSupervisionDocumentationUrl,
                                technicalSupervisionDocumentationUrl: $scope.usage.TechnicalSupervisionDocumentationUrl,
                                userSupervisionDocumentationUrl: $scope.usage.UserSupervisionDocumentationUrl,
                                riskSupervisionDocumentationUrl: $scope.usage.RiskSupervisionDocumentationUrl,
                                dataHearingSupervisionDocumentationUrl: $scope.usage.DataHearingSupervisionDocumentationUrl,
                                DPIASupervisionDocumentationUrl: $scope.usage.DPIASupervisionDocumentationUrl
                            }

                            switch (field) {
                                case 'datahandlerSupervisionDocumentationUrl':
                                    payload.datahandlerSupervisionDocumentationUrl = $scope.Url;
                                    break;
                                case 'technicalSupervisionDocumentationUrl':
                                    payload.technicalSupervisionDocumentationUrl = $scope.Url;
                                    break;
                                case 'UserSupervisionDocumentationUrl':
                                    payload.userSupervisionDocumentationUrl = $scope.Url;
                                    break;
                                case 'RiskSupervisionDocumentationUrl':
                                    payload.riskSupervisionDocumentationUrl = $scope.Url;
                                    break;
                                case 'DataHearingSupervisionDocumentationUrl':
                                    payload.dataHearingSupervisionDocumentationUrl = $scope.Url;
                                    break;
                                case 'DPIASupervisionDocumentationUrl':
                                    payload.DPIASupervisionDocumentationUrl = $scope.Url;
                                    break;
                            }
                      
                            var msg = notify.addInfoMessage("Gemmer...", false);

                            $http({ method: 'PATCH', url: 'api/itsystemusage/' + $scope.usage.Id + '?organizationId=' + user.currentOrganizationId, data: payload })
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
                        usage: [function () { return $scope.systemUsage;}]
                    }
                })
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

            $scope.datepickerOptions = {
                format: "dd-MM-yyyy"
            };
        }]);

})(angular, app);
