(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-contract.edit.economy", {
            url: "/economy",
            templateUrl: "app/components/it-contract/tabs/it-contract-tab-economy.view.html",
            controller: "contract.EditEconomyCtrl",
            resolve: {
                orgUnits: ["$http", "userService", function ($http, userService) {
                    return userService.getUser().then(function (user) {
                        return $http.get("api/organizationunit/?organizationid=" + user.currentOrganizationId).then(function (result) {
                            return result.data.response;
                        });
                    });
                }],
                externalEconomyStreams: ["$http", "contract", "$state", function ($http, contract) {
                    return $http.get(`api/EconomyStream/?externPaymentForContractWithId=${contract.id}`).then(function (result) {
                        return result.data.response;
                    }, function (error) {
                        return error;
                    });
                }],
                internalEconomyStreams: ["$http", "contract", "$state", function ($http, contract) {
                    return $http.get(`api/EconomyStream/?internPaymentForContractWithId=${contract.id}`).then(function (result) {
                        return result.data.response;
                    }, function (error) {
                        return error;
                    });
                }]
            }
        });
    }]);

    app.controller("contract.EditEconomyCtrl", ["$scope", "$http", "$timeout", "$state", "$stateParams", "notify",
        "contract", "orgUnits", "user", "externalEconomyStreams", "internalEconomyStreams", "_", "hasWriteAccess",
        function ($scope, $http, $timeout, $state, $stateParams, notify, contract, orgUnits: { ean; }[], user, externalEconomyStreams, internalEconomyStreams, _, hasWriteAccess) {

            $scope.hasWriteAccess = hasWriteAccess;

            if (externalEconomyStreams.status === 401 && internalEconomyStreams.status === 401) {
                notify.addInfoMessage("Du har ikke lov til at se disse informationer. Kontakt venligst din lokale administrator eller kontrakt administrator.");
            } else {
                var baseUrl = "api/economyStream";
                $scope.datepickerOptions = {
                    format: "dd-MM-yyyy",
                    parseFormats: ["yyyy-MM-dd"]
                };

                var allStreams = [];
                _.each(externalEconomyStreams,
                    function (stream) {
                        allStreams.push(stream);
                    });

                _.each(internalEconomyStreams, function (stream) {
                    allStreams.push(stream);
                });

                if (allStreams.length > 0) {
                    $scope.visibility = allStreams[0].accessModifier;
                } else {
                    $scope.visibility = 0;
                }

                var externEconomyStreams = [];
                $scope.externEconomyStreams = externEconomyStreams;
                _.each(externalEconomyStreams, function (stream) {
                    pushStream(stream, externEconomyStreams);
                });

                var internEconomyStreams = [];
                $scope.internEconomyStreams = internEconomyStreams;
                _.each(internalEconomyStreams, function (stream) {
                    pushStream(stream, internEconomyStreams);
                });

                $scope.changeVisibility = function () {
                    if ($scope.hasWriteAccess) {
                        if (externalEconomyStreams.length !== 0) {
                            _.each(externalEconomyStreams,
                                function (stream) {
                                    patchEconomyStream(stream).then(function () {
                                        notify.addSuccessMessage("Synligheden for ekstern betaling blev opdateret");
                                    }, function (error) {
                                        if (error.status === 403) {
                                            notify.addInfoMessage("Du har ikke lov til at foretage denne handling.");
                                        } else {
                                            notify.addErrorMessage("Synligheden for ekstern betaling blev ikke opdateret");
                                        }
                                    });
                                });
                        }

                        if (internalEconomyStreams !== 0) {
                            _.each(internalEconomyStreams,
                                function (stream) {
                                    patchEconomyStream(stream).then(function () {
                                        notify.addSuccessMessage("Synligheden for intern betaling blev opdateret");
                                    }, function (error) {
                                        if (error.status === 403) {
                                            notify.addInfoMessage("Du har ikke lov til at foretage denne handling.");
                                        } else {
                                            notify.addErrorMessage("Synligheden for intern betaling blev ikke opdateret");
                                        }
                                    });
                                });
                        }

                    }
                }

                function patchEconomyStream(stream) {
                    const payload = {
                        "accessModifier": `${$scope.visibility}`
                    };
                    return $http.patch(`api/EconomyStream/?id=${stream.id}&organizationId=${user.currentOrganizationId}`, payload);
                }

                function pushStream(stream, collection) {
                    stream.show = true;
                    stream.updateUrl = baseUrl + "/" + stream.id;

                    stream.delete = function () {
                        var msg = notify.addInfoMessage("Sletter række...");

                        $http.delete(this.updateUrl + "?organizationId=" + user.currentOrganizationId)
                            .then(function onSuccess(result) {
                                stream.show = false;

                                msg.toSuccessMessage("Rækken er slettet!");
                            }, function onError(result) {
                                msg.toErrorMessage("Fejl! Kunne ikke slette rækken!");
                            }).finally(reload);
                    };

                    function updateEan() {
                        stream.ean = " - ";

                        if (stream.organizationUnitId) {
                            var orgUnit: { ean } = _.find(orgUnits, { id: parseInt(stream.organizationUnitId) });

                            if (orgUnit && orgUnit.ean) stream.ean = orgUnit.ean;
                        }
                    };
                    stream.updateEan = updateEan;

                    updateEan();
                    collection.push(stream);
                }

                function postStream(field, organizationId) {
                    var stream = {};
                    stream[field] = contract.id;
                    stream[organizationId] = user.currentOrganizationId;

                    var msg = notify.addInfoMessage("Tilføjer ny række...");
                    $http.post(`api/EconomyStream/?contractId=${contract.id}`, stream)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Rækken er tilføjet!");
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl! Kunne ikke tilføje række");
                        }).finally(reload);
                }

                $scope.newExtern = function () {
                    postStream("ExternPaymentForId", "OrganizationId");
                };
                $scope.newIntern = function () {
                    postStream("InternPaymentForId", "OrganizationId");
                };
                $scope.patchDate = (field, value, id) => {
                    var date = moment(value, "DD-MM-YYYY");
                    if (value === "") {
                        var payload = {};
                        payload[field] = null;
                        patch(payload, `api/EconomyStream/?id=${id}&organizationId=${user.currentOrganizationId}`);
                    } else if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                        notify.addErrorMessage("Den indtastede dato er ugyldig.");

                    }
                    else {
                        var dateString = date.format("YYYY-MM-DD");
                        var payload = {};
                        payload[field] = dateString;
                        patch(payload, `api/EconomyStream/?id=${id}&organizationId=${user.currentOrganizationId}`);
                    }
                }
                function patch(payload, url) {
                    var msg = notify.addInfoMessage("Gemmer...", false);
                    $http({ method: 'PATCH', url: url, data: payload })
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Feltet er opdateret.");
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                        });
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
            }
        }]);

})(angular, app);