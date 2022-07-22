((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-contract.edit.deadlines", {
            url: "/deadlines",
            templateUrl: "app/components/it-contract/tabs/it-contract-tab-deadlines.view.html",
            controller: "contract.DeadlinesCtrl",
            resolve: {
                optionExtensions: ["localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                    localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.OptionExtendTypes).getAll()
                ],
                terminationDeadlines: ["localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                    localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.TerminationDeadlineTypes).getAll()
                ]
            }
        });
    }]);

    app.controller("contract.DeadlinesCtrl", ["$scope", "$http", "$timeout", "$state", "$stateParams", "notify", "optionExtensions", "terminationDeadlines", "user", "moment", "$q", "contract", "uiState",
        ($scope, $http, $timeout, $state, $stateParams, notify, optionExtensions, terminationDeadlines, user, moment, $q, contract, uiState: Kitos.Models.UICustomization.ICustomizedModuleUI) => {
            $scope.contract = contract;
            $scope.autosaveUrl = "api/itcontract/" + contract.id;
            $scope.optionExtensions = optionExtensions;
            $scope.terminationDeadlines = terminationDeadlines;
            $scope.durationYears = contract.durationYears;
            $scope.durationMonths = contract.durationMonths;
            $scope.durationOngoing = contract.durationOngoing;

            const blueprint = Kitos.Models.UICustomization.Configs.BluePrints.ItContractUiCustomizationBluePrint;

            $scope.isAgreementDurationEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.deadlines.children.agreementDuration);
            $scope.isAgreementDurationOnGoingEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.deadlines.children.agreementDurationOnGoing);
            $scope.isOptionExtendEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.deadlines.children.optionExtend);
            $scope.isOptionExtendMultiplierEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.deadlines.children.optionExtendMultiplier);
            $scope.isIrrevocableEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.deadlines.children.irrevocable);
            $scope.isAgreementTerminatedEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.deadlines.children.agreementTerminated);
            $scope.isAgreementNoticeEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.deadlines.children.agreementNotice);
            $scope.isAgreementRunningEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.deadlines.children.agreementRunning);
            $scope.isAgreementByEndingEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.deadlines.children.agreementByEnding);

            $scope.running = Kitos.Models.ItContract.YearSegmentOptions.getFromOption(contract.running);
            $scope.byEnding = Kitos.Models.ItContract.YearSegmentOptions.getFromOption(contract.byEnding);

            $scope.updateRunning = () => {
                contract.running = $scope.running?.id || null;
            }

            $scope.updateByEnding = () => {
                contract.byEnding = $scope.byEnding?.id || null;
            }

            $scope.deadlineOptions = Kitos.Models.ItContract.YearSegmentOptions.options;

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
                        contract.durationYears = $scope.durationYears;
                    }, () => {
                        $scope.durationYears = contract.durationYears;
                    });

                } else {
                    const msg = notify.addInfoMessage("Gemmer...", false);
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
                        contract.durationMonths = $scope.durationMonths;
                    }, () => {
                        $scope.durationMonths = contract.durationMonths;
                    });

                } else {
                    const msg = notify.addInfoMessage("Gemmer...", false);
                    msg.toErrorMessage("Antallet af måneder er ikke gyldig.");
                }
                cleanUp();
            };

            $scope.saveOngoingStatus = () => {
                const payload = {
                    "DurationYears": 0,
                    "DurationMonths": 0,
                    "DurationOngoing": $scope.durationOngoing
                };
                var msg = notify.addInfoMessage("Gemmer...", false);
                $http.patch(`odata/itcontracts(${contract.id})`, payload)
                    .then(function onSuccess(result) {
                        msg.toSuccessMessage("Varigheden blev gemt.");
                        $scope.durationYears = "";
                        $scope.durationMonths = "";

                        //it is done this way so '0' doesnt appear in input
                        contract.durationOngoing = $scope.durationOngoing;
                        contract.durationYears = $scope.durationYears;
                        contract.durationMonths = $scope.durationMonths;

                    }, function onError(result) {
                        msg.toErrorMessage("Varigheden blev ikke gemt.");
                    });

            }

            function saveDuration(payload) {
                const deferred = $q.defer();
                var msg = notify.addInfoMessage("Gemmer...", false);
                $http.patch(`odata/itcontracts(${contract.id})`, payload)
                    .then(function onSuccess(result) {
                        msg.toSuccessMessage("Varigheden blev gemt.");

                        deferred.resolve();

                    }, function onError(result) {
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

            $scope.patchDate = (field, value) => {
                var date = moment(value, Kitos.Constants.DateFormat.DanishDateFormat);
                if (value === "") {
                    var payload = {};
                    payload[field] = null;
                    patch(payload, $scope.autosaveUrl + '?organizationId=' + user.currentOrganizationId);
                } else if (value == null) {

                } else if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig.");

                } else {
                    const dateString = date.format("YYYY-MM-DD");
                    var payload = {};
                    payload[field] = dateString;
                    patch(payload, $scope.autosaveUrl + '?organizationId=' + user.currentOrganizationId);
                }
            }
            $scope.patchDateProcurement = (field, value, id, url) => {
                var date = moment(value, Kitos.Constants.DateFormat.DanishDateFormat);

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
                }).then(() => {
                    $scope.hideContent = true;
                    return $timeout(() => $scope.hideContent = false, 1);
                });
            };

            cleanUp();

        }]);
})(angular, app);