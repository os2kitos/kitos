(function (ng, app) {
    app.config(["$stateProvider", ($stateProvider) => {
        $stateProvider.state("it-system.usage.relation", {
            url: "/relation",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-relation.view.html",
            controller: "system.EditRelation"
        });
    }]);

    app.controller("system.EditRelation", ["$scope", "$http", "$state", "itSystemUsage", "notify", "$uibModal", "systemRelationService",
        ($scope, $http, $state, itSystemUsage, notify, $modal, systemRelationService) => {
            var usageId = itSystemUsage.id;
            $scope.usage = itSystemUsage;
            var modalOpen = false;
            const maxTextFieldCharCount = 199;
            const shortTextLineCount = 4;


            systemRelationService.getRelationsFrom(usageId)
                .then((systemRelations : [Kitos.Models.ItSystemUsage.Relation.IItSystemUsageRelationDTO]) => {
                    $scope.relationTableTestData = mapSystemRelations(systemRelations);
                });

            systemRelationService.getRelationsTo(usageId)
                .then(systemRelations => {
                    $scope.usedByRelations = mapSystemRelations(systemRelations);
                });

            function mapSystemRelations(systemRelations: [Kitos.Models.ItSystemUsage.Relation.IItSystemUsageRelationDTO])
            {
                const usedByOverviewData: Kitos.Models.ItSystemUsage.Relation.ISystemRelationViewModel[] = new Array();
                _.each(systemRelations,
                    (systemRelation) => {
                        usedByOverviewData.push(
                            new Kitos.Models.ItSystemUsage.Relation.SystemRelationViewModel(maxTextFieldCharCount,
                                shortTextLineCount,
                                systemRelation));
                    });
                return usedByOverviewData;
            }

            const reload = () => {
                $state.go(".", null, { reload: true });
            };

            $scope.createRelation = () => {
                if (modalOpen === false) {
                    modalOpen = true;

                    $modal.open({
                        windowClass: "modal fade in",
                        templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-relation-modal-view.html",
                        controller: ["$scope", "select2LoadingService", ($scope, select2LoadingService) => {
                            modalOpen = true;
                            $scope.RelationExposedSystemDataCall = select2LoadingService.loadSelect2(`api/v1/systemrelations/options/${usageId}/systems-which-can-be-related-to`, true, [`fromSystemUsageId=${usageId}`, `amount=10`], true, "nameContent");
                            $scope.interfaceOptions = "";

                            $scope.ExposedSystemSelected = () => {
                                const checkIfValueIsStillPresent = (relationOptions: Kitos.Models.ItSystemUsage.Relation.IItSystemUsageRelationOptionsDTO) => {

                                    if ($scope.relationPaymentFrequenciesValue) {

                                        for (let i = 0; i < relationOptions.availableFrequencyTypes.length; i++) {

                                            if (relationOptions.availableFrequencyTypes[i].id === $scope.relationPaymentFrequenciesValue.id) {
                                                $scope.relationPaymentFrequenciesOptions = relationOptions.availableFrequencyTypes;
                                                $scope.relationPaymentFrequenciesValue = relationOptions.availableFrequencyTypes[i];
                                                break;
                                            }
                                        }
                                        $scope.relationPaymentFrequenciesOptions = relationOptions.availableFrequencyTypes;

                                    } else {
                                        $scope.relationPaymentFrequenciesOptions = relationOptions.availableFrequencyTypes;
                                    }

                                    if ($scope.relationInterfacesValue) {

                                        for (let i = 0; i < relationOptions.availableInterfaces.length; i++) {

                                            if (relationOptions.availableInterfaces[i].id === $scope.relationInterfacesValue.id) {
                                                $scope.relationInterfaceOptions = relationOptions.availableInterfaces;
                                                $scope.relationInterfacesValue = relationOptions.availableInterfaces[i];
                                                break;
                                            }
                                        }
                                        $scope.relationInterfaceOptions = relationOptions.availableInterfaces;

                                    } else {
                                        $scope.relationInterfaceOptions = relationOptions.availableInterfaces;
                                    }

                                    if ($scope.relationContractsValue) {

                                        for (let i = 0; i < relationOptions.availableContracts.length; i++) {

                                            if (relationOptions.availableContracts[i].id === $scope.relationContractsValue.id) {
                                                $scope.relationContractsOptions = relationOptions.availableContracts;
                                                $scope.relationContractsValue = relationOptions.availableContracts[i];
                                                break;
                                            }
                                        }
                                        $scope.relationContractsOptions = relationOptions.availableContracts;

                                    } else {
                                        $scope.relationContractsOptions = relationOptions.availableContracts;
                                    }

                                }
                                if ($scope.RelationExposedSystemData != null) {
                                    systemRelationService
                                        .getAvailableRelationOptions(usageId, $scope.RelationExposedSystemData.id)
                                        .then((relationOptions: Kitos.Models.ItSystemUsage.Relation.IItSystemUsageRelationOptionsDTO) => {
                                            checkIfValueIsStillPresent(relationOptions);
                                        });
                                }
                            }

                            $scope.save = () => {
                                modalOpen = false;
                                var interfaceId = null;
                                var frequencyTypeId = null;
                                var contractId = null;
                                var reference = "";
                                var description = "";

                                if (!!$scope.relationInterfacesValue) {
                                    interfaceId = $scope.relationInterfacesValue.id;
                                }

                                if (!!$scope.relationPaymentFrequenciesValue) {
                                    frequencyTypeId = $scope.relationPaymentFrequenciesValue.id;
                                }

                                if (!!$scope.relationContractsValue) {
                                    contractId = $scope.relationContractsValue.id;
                                }
                                if (!!$scope.relationDescriptionValue) {
                                    description = $scope.relationDescriptionValue;
                                }
                                if (!!$scope.relationReferenceValue) {
                                    reference = $scope.relationReferenceValue;
                                }

                                const relation = {
                                    FromUsageId: usageId,
                                    ToUsageId: $scope.RelationExposedSystemData.id,
                                    Description: description,
                                    InterfaceId: interfaceId,
                                    FrequencyTypeId: frequencyTypeId,
                                    ContractId: contractId,
                                    Reference: reference,
                                } as Kitos.Models.ItSystemUsage.Relation.IItSystemUsageCreateRelationDTO;

                                notify.addInfoMessage("Tilføjer relation ...", true);
                                systemRelationService.createSystemRelation(relation).success(_ => {
                                    notify.addSuccessMessage("Relation tilføjet");
                                    $scope.$close(true);
                                    reload();
                                }).error(_ => {
                                    notify.addErrorMessage("Der opstod en fejl! Kunne ikke tilføje relation");
                                });

                            }

                            $scope.dismiss = () => {
                                modalOpen = false;
                                $scope.$close(true);
                            }

                            modalOpen = false;
                        }],
                    });
                }

            }

            $scope.expandParagraph = (e) => {
                var element = angular.element(e.currentTarget);
                var para = element.closest("td").find(document.getElementsByClassName("readMoreParagraph"))[0];
                var btn = element[0];

                if (para.getAttribute("style") != null) {
                    para.removeAttribute("style");
                    btn.innerText = "Se mindre";
                }
                else {
                    para.setAttribute("style", "height: " + shortTextLineCount + "em;overflow: hidden;");
                    btn.innerText = "Se mere";
                }
            }
        }]);
})(angular, app);
