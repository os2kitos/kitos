((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-contract.edit.economy", {
            url: "/economy",
            templateUrl: "app/components/it-contract/tabs/it-contract-tab-economy.view.html",
            controller: "contract.EditEconomyCtrl",
            resolve: {
                orgUnits: [
                    "$http", "contract", ($http, contract) => $http.get("api/organizationUnit?organization=" + contract.organizationId).then(result => {

                        var options: Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<number>[] = [];

                        function visit(orgUnit: Kitos.Models.Api.Organization.OrganizationUnit, indentationLevel: number) {
                            const option = {
                                id: String(orgUnit.id),
                                text: orgUnit.name,
                                indentationLevel: indentationLevel,
                                optionalExtraObject: orgUnit.ean
                            };

                            options.push(option);

                            _.each(orgUnit.children, child => visit(child, indentationLevel + 1));

                        }
                        visit(result.data.response, 0);
                        return options;
                    })
                ],
                externalEconomyStreams: ["$http", "contract", "$state",
                    ($http, contract) => $http.get(`api/EconomyStream/?externPaymentForContractWithId=${contract.id}`)
                        .then(result => result.data.response, error => error)],
                internalEconomyStreams:
                    ["$http", "contract", "$state",
                        ($http, contract) => $http.get(`api/EconomyStream/?internPaymentForContractWithId=${contract.id}`)
                            .then(result => result.data.response, error => error)],
                paymentFrequencies: ["localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                    localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.PaymentFrequencyTypes).getAll()
                ],
                paymentModels: ["localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                    localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.PaymentModelTypes).getAll()
                ],
                priceRegulations: ["localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                    localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.PriceRegulationTypes).getAll()]
            }
        });
    }]);

    app.controller("contract.EditEconomyCtrl", ["$scope", "$http", "$timeout", "$state", "$stateParams", "notify", "contract", "orgUnits", "user", "externalEconomyStreams", "internalEconomyStreams", "_", "hasWriteAccess", "paymentFrequencies", "paymentModels", "priceRegulations", "uiState",
        ($scope, $http, $timeout, $state, $stateParams, notify, contract, orgUnits: Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<number>[], user, externalEconomyStreams, internalEconomyStreams, _, hasWriteAccess, paymentFrequencies: Kitos.Models.IOptionEntity[], paymentModels: Kitos.Models.IOptionEntity[], priceRegulations: Kitos.Models.IOptionEntity[], uiState: Kitos.Models.UICustomization.ICustomizedModuleUI) => {
            $scope.orgUnits = orgUnits;
            $scope.allowClear = true;
            $scope.hasWriteAccess = hasWriteAccess;
            $scope.contract = contract;
            $scope.paymentFrequencies = paymentFrequencies;
            $scope.paymentModels = paymentModels;
            $scope.priceRegulations = priceRegulations;
            $scope.patchPaymentModelUrl = `api/itcontract/${contract.id}`;

            const blueprint = Kitos.Models.UICustomization.Configs.BluePrints.ItContractUiCustomizationBluePrint;

            $scope.isPaymentModelEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.economy.children.paymentModel);
            $scope.isOperationEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.economy.children.paymentModel.children.operation);
            $scope.isFrequencyEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.economy.children.paymentModel.children.frequency);
            $scope.isFieldPaymentModelEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.economy.children.paymentModel.children.paymentModel);
            $scope.isPriceEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.economy.children.paymentModel.children.price);
            $scope.isExtPaymentEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.economy.children.extPayment);
            $scope.isIntPaymentEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.economy.children.intPayment);

            function convertDate(value: string): moment.Moment {
                return moment(value, Kitos.Constants.DateFormat.DanishDateFormat);
            }

            function isDateInvalid(date: moment.Moment) {
                return !date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099;
            }

            $scope.patchPaymentModelDate = (field, value) => {
                function patchContract(payload, url) {
                    var msg = notify.addInfoMessage("Gemmer...", false);
                    $http({ method: "PATCH", url: url, data: payload })
                        .then(result => {
                            msg.toSuccessMessage("Feltet er opdateret.");
                        }, result => {
                            msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                        });
                }

                const date = convertDate(value);
                if (value === "") {
                    var payload = {};
                    payload[field] = null;
                    patchContract(payload, $scope.patchPaymentModelUrl + "?organizationId=" + user.currentOrganizationId);
                } else if (value == null) {

                } else if (isDateInvalid(date)) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig.");

                }
                else {
                    const dateString = date.format("YYYY-MM-DD");
                    var payload = {};
                    payload[field] = dateString;
                    patchContract(payload, $scope.patchPaymentModelUrl + "?organizationId=" + user.currentOrganizationId);
                }
            }

            var baseUrl = "api/economyStream";
            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            var allStreams = [];
            _.each(externalEconomyStreams,
                stream => {
                    allStreams.push(stream);
                });

            _.each(internalEconomyStreams, stream => {
                allStreams.push(stream);
            });

            var externEconomyStreams = [];
            $scope.externEconomyStreams = externEconomyStreams;
            _.each(externalEconomyStreams, stream => {
                pushStream(stream, externEconomyStreams);
            });

            var internEconomyStreams = [];
            $scope.internEconomyStreams = internEconomyStreams;
            _.each(internalEconomyStreams, stream => {
                pushStream(stream, internEconomyStreams);
            });

            function pushStream(stream, collection) {
                stream.show = true;
                stream.updateUrl = baseUrl + "/" + stream.id;

                stream.delete = function () {
                    const msg = notify.addInfoMessage("Sletter række...");

                    $http.delete(this.updateUrl + "?organizationId=" + user.currentOrganizationId)
                        .then(result => {
                            stream.show = false;
                            collection = _.remove(collection, (item) => item.id === stream.id);
                            msg.toSuccessMessage("Rækken er slettet!");
                        }, result => {
                            msg.toErrorMessage("Fejl! Kunne ikke slette rækken!");
                        }).finally(reload);
                };

                function updateEan() {
                    stream.ean = " - ";

                    if (stream.organizationUnitId !== null && stream.organizationUnitId !== undefined) {
                        stream.ean = stream.organizationUnitId.optionalExtraObject;
                    }
                };
                stream.updateEan = updateEan;

                updateEan();
                collection.push(stream);
            }

            function postStream(field, organizationId) {
                const stream = {};
                stream[field] = contract.id;
                stream[organizationId] = user.currentOrganizationId;

                const msg = notify.addInfoMessage("Tilføjer ny række...");
                $http.post(`api/EconomyStream/?contractId=${contract.id}`, stream)
                    .then(result => {
                        msg.toSuccessMessage("Rækken er tilføjet!");
                    }, result => {
                        msg.toErrorMessage("Fejl! Kunne ikke tilføje række");
                    }).finally(reload);
            }

            $scope.newExtern = () => {
                postStream("ExternPaymentForId", "OrganizationId");
            };
            $scope.newIntern = () => {
                postStream("InternPaymentForId", "OrganizationId");
            };
            $scope.patchDate = (field, value, id) => {
                const date = convertDate(value);
                if (value === "") {
                    var payload = {};
                    payload[field] = null;
                    patch(payload, `api/EconomyStream/?id=${id}&organizationId=${user.currentOrganizationId}`);
                } else if (isDateInvalid(date)) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig.");

                }
                else {
                    const dateString = date.format("YYYY-MM-DD");
                    var payload = {};
                    payload[field] = dateString;
                    patch(payload, `api/EconomyStream/?id=${id}&organizationId=${user.currentOrganizationId}`);
                }
            }
            function patch(payload, url) {
                const msg = notify.addInfoMessage("Gemmer...", false);
                $http({ method: "PATCH", url: url, data: payload })
                    .then(result => {
                        msg.toSuccessMessage("Feltet er opdateret.");
                    }, result => {
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
        }]);

})(angular, app);