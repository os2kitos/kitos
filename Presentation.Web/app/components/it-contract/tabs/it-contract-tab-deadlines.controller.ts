((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-contract.edit.deadlines", {
            url: "/deadlines",
            templateUrl: "app/components/it-contract/tabs/it-contract-tab-deadlines.view.html",
            controller: "contract.DeadlinesCtrl",
            controllerAs: "deadlinesVm",
            resolve: {
                optionExtensions: ["localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                    localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.OptionExtendTypes).getAll()
                ],
                terminationDeadlines: ["localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                    localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.TerminationDeadlineTypes).getAll()
                ]
            },
        });
    }]);

    app.controller("contract.DeadlinesCtrl", ["$http", "$timeout", "$state", "$stateParams", "notify", "optionExtensions", "terminationDeadlines", "user", "moment", "$q", "contract", "uiState",
        ($http, $timeout, $state, $stateParams, notify, optionExtensions, terminationDeadlines, user, moment, $q, contract, uiState: Kitos.Models.UICustomization.ICustomizedModuleUI) => {
            const vm = this; //using controllerAs, so we capture "this" and bind all properties to it
            vm.contract = contract;
            vm.autosaveUrl = "api/itcontract/" + contract.id;
            vm.optionExtensions = optionExtensions;
            vm.terminationDeadlines = terminationDeadlines;
            vm.durationYears = contract.durationYears ?? "";
            vm.durationMonths = contract.durationMonths ?? "";
            vm.durationOngoing = contract.durationOngoing === true;

            const blueprint = Kitos.Models.UICustomization.Configs.BluePrints.ItContractUiCustomizationBluePrint;

            vm.isAgreementDeadlinesEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.deadlines.children.agreementDeadlines);
            vm.isTerminationEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.deadlines.children.termination);

            vm.running = Kitos.Models.ItContract.YearSegmentOptions.getFromOption(contract.running);
            vm.byEnding = Kitos.Models.ItContract.YearSegmentOptions.getFromOption(contract.byEnding);

            vm.updateRunning = () => {
                contract.running = vm.running?.id || null;
            }

            vm.updateByEnding = () => {
                contract.byEnding = vm.byEnding?.id || null;
            }

            vm.deadlineOptions = Kitos.Models.ItContract.YearSegmentOptions.options;

            vm.saveDurationYears = () => {
                if (vm.durationYears == null || vm.durationYears === "") {
                    return;
                }
                const years = parseInt(vm.durationYears);
                if (years > -1) {
                    const payload = {
                        "DurationYears": years || 0
                    }

                    saveDuration(payload).then(() => {
                        contract.durationYears = vm.durationYears;
                    }, () => {
                        vm.durationYears = contract.durationYears;
                    });

                } else {
                    const msg = notify.addInfoMessage("Gemmer...", false);
                    msg.toErrorMessage("Antallet af år er ikke gyldig.");
                }
                cleanUp();
            };

            vm.saveDurationMonths = () => {
                if (vm.durationMonths == null || vm.durationMonths === "") {
                    return;
                }
                const months = parseInt(vm.durationMonths);
                if (months > -1 && months < 12) {
                    const payload = {
                        "DurationMonths": months || 0
                    }

                    saveDuration(payload).then(() => {
                        contract.durationMonths = vm.durationMonths;
                    }, () => {
                        vm.durationMonths = contract.durationMonths;
                    });

                } else {
                    const msg = notify.addInfoMessage("Gemmer...", false);
                    msg.toErrorMessage("Antallet af måneder er ikke gyldig.");
                }
                cleanUp();
            };

            vm.saveOngoingStatus = () => {
                const payload = {
                    "DurationYears": 0,
                    "DurationMonths": 0,
                    "DurationOngoing": vm.durationOngoing
                };
                var msg = notify.addInfoMessage("Gemmer...", false);
                $http.patch(`odata/itcontracts(${contract.id})`, payload)
                    .then(function onSuccess(result) {
                        msg.toSuccessMessage("Varigheden blev gemt.");
                        vm.durationYears = "";
                        vm.durationMonths = "";

                        //it is done this way so '0' doesnt appear in input
                        contract.durationOngoing = vm.durationOngoing;
                        contract.durationYears = vm.durationYears;
                        contract.durationMonths = vm.durationMonths;

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
                const years = parseInt(vm.durationYears);
                const months = parseInt(vm.durationMonths);

                if (years === 0 || years < 0) {
                    vm.durationYears = "";
                }

                if (months === 0 || months < 0 || months > 11) {
                    vm.durationMonths = "";
                }
            }

            vm.datepickerOptions = Kitos.Configs.standardKendoDatePickerOptions;

            vm.patchDate = (field, value, fieldName) => {
                var payload = {};
                const url = vm.autosaveUrl + "?organizationId=" + user.currentOrganizationId;

                if (!value) {
                    payload[field] = null;
                    patch(payload, url);
                }
                else if (Kitos.Helpers.DateValidationHelper.validateDateInput(value, notify, fieldName, true)) {
                    const dateString = Kitos.Helpers.DateStringFormat.fromDanishToEnglishFormat(value);
                    payload[field] = dateString;
                    patch(payload, url);
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
                    vm.hideContent = true;
                    return $timeout(() => vm.hideContent = false, 1);
                });
            };

            cleanUp();

            return vm; //Return the captured vm context

        }]);
})(angular, app);