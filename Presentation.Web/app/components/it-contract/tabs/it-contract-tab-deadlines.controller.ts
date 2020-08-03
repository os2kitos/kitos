(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-contract.edit.deadlines", {
            url: "/deadlines",
            templateUrl: "app/components/it-contract/tabs/it-contract-tab-deadlines.view.html",
            controller: "contract.DeadlinesCtrl",
            resolve: {
                optionExtensions: ["$http", function ($http) {
                    return $http.get("odata/LocalOptionExtendTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(function (result) {
                        return result.data.value;
                    });
                }],
                terminationDeadlines: ["$http", function ($http) {
                    return $http.get("odata/LocalTerminationDeadlineTypes").then(function (result) {
                        return result.data.value;
                    });
                }],
                paymentMilestones: ["$http", "$stateParams", function ($http, $stateParams) {
                    return $http.get("api/paymentMilestone/" + $stateParams.id + "?contract=true").then(function (result) {
                        return result.data.response;
                    });
                }],
                handoverTrialTypes: ["localOptionServiceFactory", (localOptionServiceFactory : Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                    localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.HandoverTrialTypes).getAll()],
                handoverTrials: ["$http", "$stateParams", function ($http, $stateParams) {
                    return $http.get("api/handoverTrial/" + $stateParams.id + "?byContract=true").then(function (result) {
                        return result.data.response;
                    });
                }]
            }
        });
    }]);

    app.controller("contract.DeadlinesCtrl", ["$scope", "$http", "$timeout", "$state", "$stateParams", "notify", "optionExtensions", "terminationDeadlines", "paymentMilestones", "handoverTrialTypes", "handoverTrials", "user", "moment", "$q",
        function ($scope, $http, $timeout, $state, $stateParams, notify, optionExtensions, terminationDeadlines, paymentMilestones, handoverTrialTypes, handoverTrials, user, moment, $q) {
            $scope.autosaveUrl = "api/itcontract/" + $scope.contract.id;
            $scope.optionExtensions = optionExtensions;
            $scope.terminationDeadlines = terminationDeadlines;
            $scope.paymentMilestones = paymentMilestones;
            $scope.handoverTrialTypes = handoverTrialTypes;
            $scope.handoverTrials = handoverTrials;
            $scope.durationYears = $scope.contract.durationYears;
            $scope.durationMonths = $scope.contract.durationMonths;
            $scope.durationOngoing = $scope.contract.durationOngoing;

            $scope.saveDurationYears = () => {
                if ($scope.durationYears === "") {
                    return;
                }
                const years = parseInt($scope.durationYears);
                if (years > -1) {
                    const payload = {
                        "DurationYears": years || 0
                    }

                    saveDuration(payload).then(() => {
                        $scope.contract.durationYears = $scope.durationYears;
                    }, () => {
                        $scope.durationYears = $scope.contract.durationYears;
                    });

                } else {
                    var msg = notify.addInfoMessage("Gemmer...", false);
                    msg.toErrorMessage("Antallet af år er ikke gyldig.");
                }
                cleanUp();
            };

            $scope.saveDurationMonths = () => {
                if ($scope.durationMonths === "") {
                    return;
                }
                const months = parseInt($scope.durationMonths);
                if (months > -1 && months < 12) {
                    const payload = {
                        "DurationMonths": months || 0
                    }

                    saveDuration(payload).then(() => {
                        $scope.contract.durationMonths = $scope.durationMonths;
                    }, () => {
                        $scope.durationMonths = $scope.contract.durationMonths;
                    });

                } else {
                    var msg = notify.addInfoMessage("Gemmer...", false);
                    msg.toErrorMessage("Antallet af måneder er ikke gyldig.");
                }
                cleanUp();
            };

            $scope.saveOngoingStatus = () => {
                let payload = {
                    "DurationYears": 0,
                    "DurationMonths": 0,
                    "DurationOngoing": $scope.durationOngoing
                }
                var msg = notify.addInfoMessage("Gemmer...", false);
                $http.patch(`odata/itcontracts(${$scope.contract.id})`, payload).success(() => {
                    msg.toSuccessMessage("Varigheden blev gemt.");
                    $scope.durationYears = "";
                    $scope.durationMonths = "";

                    //it is done this way so '0' doesnt appear in input
                    $scope.contract.durationOngoing = $scope.durationOngoing;
                    $scope.contract.durationYears = $scope.durationYears;
                    $scope.contract.durationMonths = $scope.durationMonths;

                }).error(() => {
                    msg.toErrorMessage("Varigheden blev ikke gemt.");
                });

            }

            function saveDuration(payload) {
                const deferred = $q.defer();
                var msg = notify.addInfoMessage("Gemmer...", false);
                $http.patch(`odata/itcontracts(${$scope.contract.id})`, payload).success(() => {
                    msg.toSuccessMessage("Varigheden blev gemt.");

                    deferred.resolve();

                }).error(() => {
                    msg.toErrorMessage("Varigheden blev ikke gemt.");

                    deferred.reject();
                });

                return deferred.promise;
            }

            function cleanUp() {
                const years = parseInt($scope.durationYears);
                const months = parseInt($scope.durationMonths);

                if (years === 0 || years < 0) {
                    $scope.durationYears = "";
                }

                if (months === 0 || months < 0 || months > 11) {
                    $scope.durationMonths = "";
                }
            }

            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            $scope.saveMilestone = function (paymentMilestone) {
                paymentMilestone.itContractId = $scope.contract.id;

                var approvedDate = moment(paymentMilestone.approved, "DD-MM-YYYY");
                var expectedDate = moment(paymentMilestone.expected, "DD-MM-YYYY");
                var approvedDateValid = (approvedDate.isValid() || isNaN(approvedDate.valueOf()) || approvedDate.year() < 1000 || approvedDate.year() > 2099);
                var expectedDateValid = (expectedDate.isValid() || isNaN(expectedDate.valueOf()) || expectedDate.year() < 1000 || expectedDate.year() > 2099);
                if (approvedDateValid) {
                    paymentMilestone.approved = approvedDate.format("YYYY-MM-DD");
                } else {
                    notify.addInfoMessage("Den indtastede forventet dato er ugyldig. Tom værdi indsættes");
                    paymentMilestone.approved = null;
                }
                if (expectedDateValid) {
                    paymentMilestone.expected = expectedDate.format("YYYY-MM-DD");
                } else {
                    notify.addInfoMessage("Den indtastede godkendt dato er ugyldig. Tom værdi indsættes");
                    paymentMilestone.expected = null;
                }

                var msg = notify.addInfoMessage("Gemmer...", false);
                $http.post("api/paymentMilestone", paymentMilestone)
                    .success(function (result) {
                        msg.toSuccessMessage("Gemt");
                        var obj = result.response;
                        $scope.paymentMilestones.push(obj);
                        delete $scope.paymentMilestone; // clear input fields
                        $scope.milestoneForm.$setPristine();
                    })
                    .error(function () {
                        msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                    });
            };

            $scope.deleteMilestone = function (id) {
                var msg = notify.addInfoMessage("Sletter...", false);
                $http.delete("api/paymentMilestone/" + id + "?organizationId=" + user.currentOrganizationId)
                    .success(function () {
                        msg.toSuccessMessage("Slettet");
                        reload();
                    })
                    .error(function () {
                        msg.toErrorMessage("Fejl! Kunne ikke slette!");
                    });
            };

            $scope.saveTrial = function (handoverTrial) {
                handoverTrial.itContractId = $scope.contract.id;

                var approvedDate = moment(handoverTrial.approved, "DD-MM-YYYY");
                var expectedDate = moment(handoverTrial.expected, "DD-MM-YYYY");
                var approvedDateValid = (approvedDate.isValid() || isNaN(approvedDate.valueOf()) || approvedDate.year() < 1000 || approvedDate.year() > 2099);
                var expectedDateValid = (expectedDate.isValid() || isNaN(expectedDate.valueOf()) || expectedDate.year() < 1000 || expectedDate.year() > 2099);

                if (approvedDateValid) {
                    handoverTrial.approved = approvedDate.format("YYYY-MM-DD");
                } else {
                    notify.addInfoMessage("Den indtastede forventet dato er ugyldig. Tom værdi indsættes");
                    handoverTrial.approved = null;
                }

                if (expectedDateValid) {
                    handoverTrial.expected = expectedDate.format("YYYY-MM-DD");
                } else {
                    notify.addInfoMessage("Den indtastede godkent dato er ugyldig. Tom værdi indsættes");
                    handoverTrial.expected = null;
                }

                var msg = notify.addInfoMessage("Gemmer...", false);
                $http.post("api/handoverTrial", handoverTrial)
                    .success(function (result) {
                        msg.toSuccessMessage("Gemt");
                        var obj = result.response;
                        $scope.handoverTrials.push(obj);
                        delete $scope.handoverTrial; // clear input fields
                        $scope.trialForm.$setPristine();
                    })
                    .error(function () {
                        msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                    });
            };

            $scope.deleteTrial = function (id) {
                var msg = notify.addInfoMessage("Sletter...", false);
                $http.delete("api/handoverTrial/" + id + "?organizationId=" + user.currentOrganizationId)
                    .success(function () {
                        msg.toSuccessMessage("Slettet");
                        reload();
                    })
                    .error(function () {
                        msg.toErrorMessage("Fejl! Kunne ikke slette!");
                    });
            };

            $scope.patchDate = (field, value) => {
                var date = moment(value, "DD-MM-YYYY");
                if (value === "") {
                    var payload = {};
                    payload[field] = null;
                    patch(payload, $scope.autosaveUrl + '?organizationId=' + user.currentOrganizationId);
                } else if (value == null) {

                } else if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig.");

                } else {
                    var dateString = date.format("YYYY-MM-DD");
                    var payload = {};
                    payload[field] = dateString;
                    patch(payload, $scope.autosaveUrl + '?organizationId=' + user.currentOrganizationId);
                }
            }
            $scope.patchDateProcurement = (field, value, id, url) => {
                var date = moment(value, "DD-MM-YYYY");

                if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig.");

                } else {
                    var dateString = date.format("YYYY-MM-DD");
                    var payload = {};
                    payload[field] = dateString;
                    patch(payload, url + id + '?organizationId=' + user.currentOrganizationId);
                }
            }
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

            cleanUp();

        }]);
})(angular, app);