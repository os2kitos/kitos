(function (ng, app) {
    app.config(["$stateProvider", ($stateProvider) => {
        $stateProvider.state("it-system.usage.relation", {
            url: "/relation",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-relation.view.html",
            controller: "system.EditRelation"
        });
    }]);

    app.controller("system.EditRelation", ["$scope", "itSystemUsage", "notify", "$uibModal", "systemRelationService",
        ($scope, itSystemUsage, notify, $modal, systemRelationService) => {
            var usageId = itSystemUsage.id;
            $scope.usage = itSystemUsage;
            var modalOpen = false;
            const maxTextFieldCharCount = 199;
            const shortTextLineCount = 4;
            const reload = () => {
                systemRelationService.getRelationsFrom(usageId)
                    .then((systemRelations: [Kitos.Models.ItSystemUsage.Relation.IItSystemUsageRelationDTO]) => {
                        $scope.usageRelations = mapSystemRelations(systemRelations);
                    });

                systemRelationService.getRelationsTo(usageId)
                    .then(systemRelations => {
                        $scope.usedByRelations = mapSystemRelations(systemRelations);
                    });
            };
            reload();

            function mapSystemRelations(systemRelations: [Kitos.Models.ItSystemUsage.Relation.IItSystemUsageRelationDTO]) {
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

            $scope.createRelation = () => {
                if (modalOpen === false) {
                    modalOpen = true;
                    $modal.open({
                        windowClass: "modal fade in",
                        templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-relation-modal-view.html",
                        controller: ["$scope", "select2LoadingService", ($scope, select2LoadingService) => {
                            modalOpen = true;
                            $scope.RelationExposedSystemDataCall = select2LoadingService.loadSelect2(`api/v1/systemrelations/options/${usageId}/systems-which-can-be-related-to`, true, [`fromSystemUsageId=${usageId}`, `amount=10`], true, "nameContent");
                            $scope.RelationModalState = "Opret relation for  " + itSystemUsage.itSystem.name;
                            $scope.RelationModalViewModel = new Kitos.Models.ItSystemUsage.Relation.EditSystemRelationModalViewModel(usageId, itSystemUsage.itSystem.name);

                            $scope.ContractOptions = select2LoadingService.select2LocalData(() => $scope.RelationModalViewModel.contractOptions.options);
                            $scope.InterfaceOptions = select2LoadingService.select2LocalDataNoSearch(() => $scope.RelationModalViewModel.interfaceOptions.options);
                            $scope.FrequencyOptions = select2LoadingService.select2LocalDataNoSearch(() => $scope.RelationModalViewModel.frequencyOptions.options);

                            const exposedSystemChanged = () => {
                                if ($scope.RelationModalViewModel.toSystem != null) {
                                    systemRelationService
                                        .getAvailableRelationOptions(usageId, $scope.RelationModalViewModel.toSystem.id)
                                        .then((relationOptions: Kitos.Models.ItSystemUsage.Relation.IItSystemUsageRelationOptionsDTO) => {
                                            const updatedView = $scope.RelationModalViewModel;
                                            updatedView.updateAvailableOptions(relationOptions);
                                            $scope.RelationModalViewModel = updatedView;
                                        });
                                }
                            };

                            $scope.ExposedSystemSelectedTrigger = () => {
                                exposedSystemChanged();
                            };

                            $scope.save = () => {
                                const newRelation = new Kitos.Models.ItSystemUsage.Relation.SystemRelationModelPostDataObject($scope.RelationModalViewModel);
                                notify.addInfoMessage("Tilføjer relation ...", true);
                                systemRelationService.createSystemRelation(newRelation)
                                    .then(
                                        () => {
                                            notify.addSuccessMessage("´Relation tilføjet");
                                            modalOpen = false;
                                            $scope.$close(true);
                                            reload();
                                        },
                                        error => {
                                            notify.addErrorMessage("Der opstod en fejl! Kunne ikke tilføje relation");
                                        });

                            };

                            $scope.dismiss = () => {
                                modalOpen = false;
                                $scope.$close(true);
                            };

                            modalOpen = false;
                        }],
                    });
                }

            };

            $scope.editRelation = (relationId) => {
                if (modalOpen === false) {
                    modalOpen = true;

                    $modal.open({
                        windowClass: "modal fade in",
                        templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-relation-modal-view.html",
                        controller: ["$scope", "select2LoadingService", ($scope, select2LoadingService) => {
                            modalOpen = true;
                            $scope.RelationExposedSystemDataCall = select2LoadingService.loadSelect2(`api/v1/systemrelations/options/${usageId}/systems-which-can-be-related-to`, true, [`fromSystemUsageId=${usageId}`, `amount=10`], true, "nameContent");
                            $scope.RelationModalState = "Redigere relation";

                            $scope.RelationModalViewModel = new Kitos.Models.ItSystemUsage.Relation.EditSystemRelationModalViewModel(usageId, itSystemUsage.itSystem.name);

                            $scope.ContractOptions = select2LoadingService.select2LocalData(() => $scope.RelationModalViewModel.contractOptions.options);
                            $scope.InterfaceOptions = select2LoadingService.select2LocalDataNoSearch(() => $scope.RelationModalViewModel.interfaceOptions.options);
                            $scope.FrequencyOptions = select2LoadingService.select2LocalDataNoSearch(() => $scope.RelationModalViewModel.frequencyOptions.options);

                            systemRelationService.getRelation(usageId, relationId)
                                .then((response: {
                                    error: boolean,
                                    data: Kitos.Models.ItSystemUsage.Relation.IItSystemUsageRelationDTO
                                }) => {
                                    if (response.error) {
                                        notify.addErrorMessage("Det var ikke muligt at redigere denne relation");
                                        modalOpen = false;
                                        $scope.$close(true);
                                        return;
                                    }

                                    var relation = response.data;

                                    var modalModelView = $scope.RelationModalViewModel;
                                    modalModelView.setTargetSystem(relation.toUsage.id, relation.toUsage.name);
                                    $scope.RelationModalViewModel = modalModelView;

                                    const exposedSystemChanged = () => {
                                        if ($scope.RelationModalViewModel.toSystem != null) {
                                            systemRelationService
                                                .getAvailableRelationOptions(usageId, $scope.RelationModalViewModel.toSystem.id)
                                                .then((relationOptions: Kitos.Models.ItSystemUsage.Relation.IItSystemUsageRelationOptionsDTO) => {
                                                    const updatedView = $scope.RelationModalViewModel;
                                                    updatedView.updateAvailableOptions(relationOptions);
                                                    modalModelView.setValuesFrom(relation);
                                                    $scope.RelationModalViewModel = updatedView;
                                                });
                                        }
                                    };
                                    $scope.ExposedSystemSelectedTrigger = () => {
                                        exposedSystemChanged();
                                    };
                                    exposedSystemChanged();
                                });

                            $scope.save = () => {
                                var data = $scope.RelationModalViewModel;
                                const patchRelation = new Kitos.Models.ItSystemUsage.Relation.SystemRelationModelPatchDataObject(data);
                                notify.addInfoMessage("Tilføjer relation ...", true);
                                systemRelationService.patchSystemRelation(patchRelation)
                                    .then(success => {
                                        notify.addSuccessMessage("Relation ændret");
                                        modalOpen = false;
                                        $scope.$close(true);
                                        reload();
                                    },
                                        error => {
                                            notify.addErrorMessage("Der opstod en fejl! Kunne ikke redigere relation");
                                        });
                            };

                            $scope.delete = () => {
                                systemRelationService.deleteSystemRelation(usageId, $scope.RelationModalViewModel.id)
                                    .then(success => {
                                        notify.addSuccessMessage("Relation slettet");
                                        modalOpen = false;
                                        $scope.$close(true);
                                        reload();
                                    },
                                        error => {
                                            notify.addErrorMessage("Kunne ikke slette relation");
                                        });
                            };
                            $scope.dismiss = () => {
                                modalOpen = false;
                                $scope.$close(true);
                            };
                            modalOpen = false;
                        }],
                    });
                }
            };

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
            };
        }]);
})(angular, app);
