((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-contract.edit.economy", {
            url: "/economy",
            templateUrl: "app/components/it-contract/tabs/it-contract-tab-economy.view.html",
            controller: "contract.EditEconomyCtrl",
            controllerAs: "contractEconomyVm",
            resolve: {
                orgUnits: [
                    "$http", "contract", ($http, contract) => $http.get("api/organizationUnit?organization=" + contract.organizationId).then(result => {

                        var options: Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<number>[] = [];

                        function visit(orgUnit: Kitos.Models.Api.Organization.OrganizationUnit, indentationLevel: number) {
                            const option = {
                                id: String(orgUnit.id),
                                text: orgUnit.name,
                                indentationLevel: indentationLevel,
                                optionalObjectContext: orgUnit.ean
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

    app.controller("contract.EditEconomyCtrl", ["$http", "$timeout", "$state", "$stateParams", "notify", "contract", "orgUnits", "user", "externalEconomyStreams", "internalEconomyStreams", "_", "hasWriteAccess", "paymentFrequencies", "paymentModels", "priceRegulations", "uiState",
        ($http, $timeout, $state, $stateParams, notify, contract, orgUnits: Kitos.Models.ViewModel.Generic.Select2OptionViewModelWithIndentation<number>[], user, externalEconomyStreams, internalEconomyStreams, _, hasWriteAccess, paymentFrequencies: Kitos.Models.IOptionEntity[], paymentModels: Kitos.Models.IOptionEntity[], priceRegulations: Kitos.Models.IOptionEntity[], uiState: Kitos.Models.UICustomization.ICustomizedModuleUI) => {
            const vm = this; //capture this to bind properties to it

            vm.orgUnits = orgUnits;
            vm.allowClear = true;
            vm.hasWriteAccess = hasWriteAccess;
            vm.contract = contract;
            vm.paymentFrequencies = paymentFrequencies;
            vm.paymentModels = paymentModels;
            vm.priceRegulations = priceRegulations;
            vm.patchPaymentModelUrl = `api/itcontract/${contract.id}`;

            const blueprint = Kitos.Models.UICustomization.Configs.BluePrints.ItContractUiCustomizationBluePrint;

            vm.isPaymentModelEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.economy.children.paymentModel);
            vm.isExtPaymentEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.economy.children.extPayment);
            vm.isIntPaymentEnabled = uiState.isBluePrintNodeAvailable(blueprint.children.economy.children.intPayment);
            
            vm.patchPaymentModelDate = (field, value) => {
                function patchContract(payload, url) {
                    var msg = notify.addInfoMessage("Gemmer...", false);
                    $http({ method: "PATCH", url: url, data: payload })
                        .then(result => {
                            msg.toSuccessMessage("Feltet er opdateret.");
                        }, result => {
                            msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                        });
                }

                var payload = {};
                const url = vm.patchPaymentModelUrl + "?organizationId=" + user.currentOrganizationId;

                if (value === "") {
                    payload[field] = null;
                    patchContract(payload, url);
                } else if (value == null) {

                } else if (Kitos.Helpers.DateValidationHelper.validateDateInput(value, notify, "Driftsvederlag påbegyndt", false)){
                    const dateString = Kitos.Helpers.DateStringFormat.fromDanishToEnglishFormat(value);
                    payload[field] = dateString;
                    patchContract(payload, url);
                }
            }

            var baseUrl = "api/economyStream";
            vm.datepickerOptions = Kitos.Configs.standardKendoDatePickerOptions;

            var allStreams = [];
            _.each(externalEconomyStreams,
                stream => {
                    allStreams.push(stream);
                });

            _.each(internalEconomyStreams, stream => {
                allStreams.push(stream);
            });

            var externEconomyStreams = [];
            vm.externEconomyStreams = externEconomyStreams;
            _.each(externalEconomyStreams, stream => {
                pushStream(stream, externEconomyStreams);
            });

            var internEconomyStreams = [];
            vm.internEconomyStreams = internEconomyStreams;
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
                        stream.ean = stream.organizationUnitId.optionalObjectContext;
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

            vm.newExtern = () => {
                postStream("ExternPaymentForId", "OrganizationId");
            };
            vm.newIntern = () => {
                postStream("InternPaymentForId", "OrganizationId");
            };
            vm.patchDate= (field, value, id, fieldName) => {
                const url = `api/EconomyStream/?id=${id}&organizationId=${user.currentOrganizationId}`;
                var payload = {};

                if (!value) {
                    payload[field] = null;
                    patch(payload, url);
                } else if (Kitos.Helpers.DateValidationHelper.validateDateInput(value, notify, fieldName, true)){
                    const dateString = Kitos.Helpers.DateStringFormat.fromDanishToEnglishFormat(value);
                    payload[field] = dateString;
                    patch(payload, url);
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
                    vm.hideContent = true;
                    return $timeout(() => vm.hideContent = false, 1);
                });
            };

            return vm; //Return the vm
        }]);

})(angular, app);