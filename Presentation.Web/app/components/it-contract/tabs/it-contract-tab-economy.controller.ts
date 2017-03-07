﻿(function (ng, app) {
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
                    }, function () {
                        return null;
                    });
                }],
                internalEconomyStreams: ["$http", "contract", "$state", function ($http, contract) {
                    return $http.get(`api/EconomyStream/?internPaymentForContractWithId=${contract.id}`).then(function (result) {
                        return result.data.response;
                    }, function () {
                        return null;
                    });
                }]
            }
        });
    }]);

    app.controller("contract.EditEconomyCtrl", ["$scope", "$http", "$timeout", "$state", "$stateParams", "notify",
        "contract", "orgUnits", "user", "hasWriteAccess", "externalEconomyStreams", "internalEconomyStreams", "_",
        function ($scope, $http, $timeout, $state, $stateParams, notify, contract, orgUnits: { ean; }[], user, hasWriteAccess, externalEconomyStreams, internalEconomyStreams, _) {
            if (externalEconomyStreams === null && internalEconomyStreams == null) {
                notify.addInfoMessage("Du har ikke lov til at se disse informationer. Kontakt venligst din lokale administrator eller kontrakt administrator.");
            }

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
                if (hasWriteAccess) {
                    let msg = notify.addInfoMessage("Opdaterer betalingernes synlighed...", true);
                    if (externalEconomyStreams.length !== 0) {
                        _.each(externalEconomyStreams,
                            function (stream) {
                                patchEconomyStream(stream).then(function () {
                                    msg.toSuccessMessage("Synligheden blev opdateret");
                                }, function () {
                                    msg.toSErrorMessage("Synligheden blev ikke opdateret");
                                });
                            });
                    }

                    if (internalEconomyStreams !== 0) {
                        _.each(internalEconomyStreams,
                            function (stream) {
                                patchEconomyStream(stream).then(function () {
                                    msg.toSuccessMessage("Synligheden blev opdateret");
                                }, function () {
                                    msg.toSErrorMessage("Synligheden blev ikke opdateret");
                                });
                            });
                    }
                } else {
                    notify.addInfoMessage("Du har ikke lov til at foretage denne handling.");
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

                    $http.delete(this.updateUrl + "?organizationId=" + user.currentOrganizationId).success(function () {
                        stream.show = false;

                        msg.toSuccessMessage("Rækken er slettet!");
                    }).error(function () {
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
                $http.post(`api/EconomyStream/?contractId=${contract.id}`, stream).success(function (result) {
                    msg.toSuccessMessage("Rækken er tilføjet!");
                }).error(function () {
                    msg.toErrorMessage("Fejl! Kunne ikke tilføje række");
                }).finally(reload);
            }

            $scope.newExtern = function () {
                postStream("ExternPaymentForId", "OrganizationId");
            };
            $scope.newIntern = function () {
                postStream("InternPaymentForId", "OrganizationId");
            };

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
        }]);
})(angular, app);