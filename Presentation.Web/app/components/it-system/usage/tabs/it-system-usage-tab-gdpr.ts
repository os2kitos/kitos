(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-system.usage.gdpr", {
            url: "/gdpr",
            templateUrl: "app/components/gdpr-views/gdpr.view.html",
            controller: "system.usage.EditGdpr",
            resolve: {
                theSystem: ['$http', '$stateParams', function ($http, $stateParams) {
                return $http.get("api/itsystem/" + $stateParams.id)
                    .then(function (result) {
                        return result.data.response;
                        });
                }],
                regularSensitiveData: ['$http', '$stateParams','theSystem', function ($http, $stateParams, theSystem) {
                    return $http.get("odata/GetRegularPersonalDataByObjectID(id=" + $stateParams.id + ")")
                        .then(function (result) {
                            return result.data.value;
                        });
                }],
                sensitivePersonalData: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("odata/GetSensitivePersonalDataByObjectID(id=" + $stateParams.id + ")")
                        .then(function (result) {
                            return result.data.value;
                        });
                }]
            }
        });
    }]);

    app.controller("it-system.edit.gdpr", ["$scope", "$http", "$timeout", "$state", "$stateParams", "$confirm", "notify", "hasWriteAccess","user", "theSystem", "regularSensitiveData","sensitivePersonalData",
        function ($scope, $http, $timeout, $state, $stateParams, $confirm, notify, hasWriteAccess, user, theSystem, regularSensitiveData, sensitivePersonalData) {

            $scope.hasWriteAccess = hasWriteAccess;
            $scope.system = theSystem;
            $scope.updateUrl = 'api/itsystem/' + theSystem.id;
            $scope.regularSensitiveData = regularSensitiveData;
            $scope.sensitivePersonalData = sensitivePersonalData;
            $scope.dataWorkers = theSystem.associatedDataWorkers;
            // select2 options for looking up organization as dataworkers
            $scope.dataWorkerSelectOptions = selectLazyLoading('api/organization', false, ['public=true', 'orgId=' + user.currentOrganizationId]);
            $scope.updateDataLevel = function (OptionId, Checked, optionType) {

                var msg = notify.addInfoMessage("Arbejder ...", false);

                if (Checked == true) {

                    var data = {
                        ObjectId: theSystem.id,
                        OptionId: OptionId,
                        OptionType: optionType
                    };

                    $http.post("Odata/AttachedOptions/", data, { handleBusy: true }).success(function (result) {
                        msg.toSuccessMessage("Feltet er Opdateret.");
                    }).error(function () {
                        msg.toErrorMessage("Fejl!");
                    });
                
                } else {
                    $http.delete("Odata/RemoveOption(id=" + OptionId + ", systemId=" + theSystem.id + ",type='" + optionType + "')").success(function () {
                        msg.toSuccessMessage("Feltet er Opdateret.");
                    }).error(function () {
                        msg.toErrorMessage("Fejl!");
                    });
                }
            }

            $scope.changeDataField = function (fieldName) {
                var data;

                switch (fieldName) {
                    case 'containsLegalInfo':
                     data = {
                        ContainsLegalInfo : $scope.system.containsLegalInfo
                    };
                    break;
                    case 'isDataTransferedToThirdCountries':
                     data = {
                            IsDataTransferedToThirdCountries: $scope.system.isDataTransferedToThirdCountries
                        };
                    break;
                }

                $http.patch("api/itsystem/" + theSystem.id + "?organizationId=" + theSystem.organizationId, data).success(function (result) {

                    notify.addSuccessMessage("Success");

                }).error(function (result) {
                    notify.addErrorMessage('Fejl!');
                });
            };

            $scope.delete = function (dataworkerId) {
                $http.delete("api/DataWorker/" + dataworkerId + "?organizationid=" + $scope.system.organizationId)
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
                                if (excludeSelf && obj.id == theSystem.id)
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
            // work around for $state.reload() not updating scope
            // https://github.com/angular-ui/ui-router/issues/582
            function reload() {
                return $state.transitionTo($state.current, $stateParams, {
                    reload: true
                }).then(function () {
                    $scope.hideContent = true;
                    return $timeout(function () {
                        return $scope.hideContent = false;
                    }, 1);
                });
            };

            $scope.save = function () {

                var data = {
                    ItSystemId: $scope.system.id,
                    DataWorkerId: $scope.selectedDataWorker.id
                }

                $http.post("api/Dataworker/", data)
                    .success(function () {
                        notify.addSuccessMessage("Projektet er tilknyttet.");
                        reload();
                    })
                    .error(function () {
                        notify.addErrorMessage("Fejl! Kunne ikke tilknytte projektet!");
                    });
            };
        }]);
})(angular, app);
