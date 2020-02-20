((ng, app) => {
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
            const reload = () => {
                const mapDataToViewmodelArray = (systemRelations: [Kitos.Models.Api.ItSystemUsage.Relation.IItSystemUsageRelationDTO]) => {
                    return Kitos.Models.ViewModel.ItSystemUsage.Relation.SystemRelationMapper.mapSystemRelationsToViewModels(
                        systemRelations,
                        Kitos.Configs.RelationTableCellParagraphSizeConfig.maxTextFieldCharCount,
                        Kitos.Configs.RelationTableCellParagraphSizeConfig.shortTextLineCount);
                }

                systemRelationService.getRelationsFrom(usageId)
                    .then((systemRelations: [Kitos.Models.Api.ItSystemUsage.Relation.IItSystemUsageRelationDTO]) => {
                        $scope.usageRelations = mapDataToViewmodelArray(systemRelations);
                    });

                systemRelationService.getRelationsTo(usageId)
                    .then((systemRelations: [Kitos.Models.Api.ItSystemUsage.Relation.IItSystemUsageRelationDTO]) => {
                        $scope.usedByRelations = mapDataToViewmodelArray(systemRelations);
                    });
            };

            reload();

            $scope.createRelation = () => {
                if (modalOpen === false) {
                    modalOpen = true;
                    $modal.open({
                        windowClass: "modal fade in",
                        templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-relation-modal-view.html",
                        controller: ["$scope", "select2LoadingService", ($scope, select2LoadingService) => {
                            modalOpen = true;
                            $scope.RelationExposedSystemDataCall = select2LoadingService.loadSelect2(`api/v1/systemrelations/options/${usageId}/systems-which-can-be-related-to`, true, [`fromSystemUsageId=${usageId}`, `amount=10`], true, "nameContent");
                            $scope.RelationModalViewModel = new Kitos.Models.ViewModel.ItSystemUsage.Relation.SystemRelationModalViewModel(usageId, itSystemUsage.itSystem.name);
                            $scope.RelationModalViewModel.configureAsNewRelationDialog();

                            $scope.ContractOptions = select2LoadingService.select2LocalData(() => $scope.RelationModalViewModel.contract.options);
                            $scope.InterfaceOptions = select2LoadingService.select2LocalDataNoSearch(() => $scope.RelationModalViewModel.interface.options);
                            $scope.FrequencyOptions = select2LoadingService.select2LocalDataNoSearch(() => $scope.RelationModalViewModel.frequency.options);

                            const exposedSystemChanged = () => {
                                if ($scope.RelationModalViewModel.toSystem != null) {
                                    systemRelationService
                                        .getAvailableRelationOptions(usageId, $scope.RelationModalViewModel.toSystem.id)
                                        .then((relationOptions: Kitos.Models.Api.ItSystemUsage.Relation.IItSystemUsageRelationOptionsDTO) => {
                                            const updatedView = $scope.RelationModalViewModel;
                                            updatedView.updateAvailableOptions(relationOptions);
                                            $scope.RelationModalViewModel = updatedView;
                                        });
                                }
                            };

                            $scope.ExposedSystemSelectedTrigger = exposedSystemChanged;

                            $scope.save = () => {
                                const newRelation = new Kitos.Models.Api.ItSystemUsage.Relation.SystemRelationModelPostDataObject($scope.RelationModalViewModel);
                                notify.addInfoMessage("Tilføjer relation ...", true);
                                systemRelationService.createSystemRelation(newRelation)
                                    .then(
                                        () => {
                                            notify.addSuccessMessage("Relation tilføjet");
                                            modalOpen = false;
                                            $scope.$close(true);
                                            reload();
                                        },
                                        error => {
                                            notify.addErrorMessage("Der opstod en fejl! Kunne ikke tilføje relationen.");
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

                            $scope.RelationModalViewModel = new Kitos.Models.ViewModel.ItSystemUsage.Relation.SystemRelationModalViewModel(usageId, itSystemUsage.itSystem.name);

                            $scope.ContractOptions = select2LoadingService.select2LocalData(() => $scope.RelationModalViewModel.contract.options);
                            $scope.InterfaceOptions = select2LoadingService.select2LocalDataNoSearch(() => $scope.RelationModalViewModel.interface.options);
                            $scope.FrequencyOptions = select2LoadingService.select2LocalDataNoSearch(() => $scope.RelationModalViewModel.frequency.options);

                            systemRelationService.getRelation(usageId, relationId)
                                .then((response: {
                                    error: boolean;
                                    data: Kitos.Models.Api.ItSystemUsage.Relation.IItSystemUsageRelationDTO
                                }
                                ) => {
                                    if (response.error) {
                                        notify.addErrorMessage("Det var ikke muligt at redigere denne relation");
                                        modalOpen = false;
                                        $scope.$close(true);
                                        return;
                                    }

                                    var relation = response.data;

                                    const loadOptions = (toUsage, onOptionsLoadedCallback) => {
                                        systemRelationService
                                            .getAvailableRelationOptions(usageId, toUsage.id)
                                            .then((relationOptions: Kitos.Models.Api.ItSystemUsage.Relation.
                                                IItSystemUsageRelationOptionsDTO) => onOptionsLoadedCallback(relationOptions));
                                    }

                                    const exposedSystemChanged = () => {
                                        var toSystem = $scope.RelationModalViewModel.toSystem;
                                        if (toSystem != null) {
                                            loadOptions(toSystem, options => $scope.RelationModalViewModel.updateAvailableOptions(options));
                                        }
                                    };
                                    $scope.ExposedSystemSelectedTrigger = () => {
                                        exposedSystemChanged();
                                    }

                                    //Preload initial options
                                    loadOptions(relation.toUsage, options => $scope.RelationModalViewModel.configureAsEditRelationDialog(relation, options));
                                });

                            $scope.save = () => {
                                var data = $scope.RelationModalViewModel;
                                const patchRelation = new Kitos.Models.Api.ItSystemUsage.Relation.SystemRelationModelPatchDataObject(data);
                                notify.addInfoMessage("Ændrer relation ...", true);
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
                            }

                            $scope.removeRelation = () => {

                                if (!confirm("Er du sikker på at du vil fjerne denne relation?")) {
                                    return;
                                }
                                systemRelationService.deleteSystemRelation(usageId, relationId)
                                    .then(success => {
                                        notify.addSuccessMessage("Relationen er slettet");
                                        modalOpen = false;
                                        $scope.$close(true);
                                        reload();
                                    },
                                        error => {
                                            notify.addErrorMessage("Kunne ikke slette relationen");
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
                Kitos.Utility.TableManipulation.expandRetractParagraphCell(e, Kitos.Configs.RelationTableCellParagraphSizeConfig.shortTextLineCount);
            };
        }]);
})(angular, app);
