((ng, app) => {
    app.config([
        "$stateProvider", $stateProvider => {
            $stateProvider.state("it-contract.edit.systems", {
                url: "/systems",
                templateUrl: "app/components/it-contract/tabs/it-contract-tab-systems.view.html",
                controller: "contract.EditSystemsCtrl",
                resolve: {
                    user: [
                        "userService", userService => userService.getUser()
                    ],
                    agreementElements: [
                        "localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                            localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.AgreementElementTypes).getAll()
                    ]
                }
            });
        }
    ]);

    app.controller("contract.EditSystemsCtrl", [
        "$scope", "$http", "$stateParams", "notify", "user", "contract", "agreementElements", "_", "$filter", "systemRelationService", "entityMapper",
        ($scope, $http, $stateParams, notify, user, contract, agreementElements, _, $filter, systemRelationService, entityMapper) => {

            $scope.autoSaveUrl = `api/itcontract/${$stateParams.id}`;
            $scope.contract = contract;

            $scope.formatSystemName = Kitos.Helpers.SystemNameFormat.apply;

            $scope.availableAgreementElements = entityMapper.mapOptionToSelect2ViewModel(agreementElements);

            $scope.selectedAgreementElements = entityMapper.mapApiResponseToSelect2ViewModel(contract.agreementElements);

            $scope.$watch("elements", function (newValue, oldValue) {
                if (_.isUndefined(newValue) || _.isUndefined(oldValue)) {
                    return;
                }
                if (newValue.length > oldValue.length) {
                    // something was added
                    var toAdd: any = _.difference(newValue, oldValue);
                    if (!_.isUndefined(toAdd)) {
                        for (var j = 0; j < toAdd.length; j++) {
                            var index = j;
                            var msg = notify.addInfoMessage("Gemmer...", false);
                            $http.post($scope.autoSaveUrl + "?organizationId=" + user.currentOrganizationId + "&elemId=" + toAdd[index].id)
                                .then(function onSuccess(result) {
                                    contract.agreementElements.push({ id: toAdd[index].id, name: toAdd[index].text });
                                    msg.toSuccessMessage("Feltet er opdateret.");
                                }, function onError(error) {
                                    msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                });
                        }
                    }
                } else if (newValue.length < oldValue.length) {
                    // something was removed
                    var toRemove: any = _.difference(oldValue, newValue);
                    if (!_.isUndefined(toRemove)) {
                        for (var i = 0; i < toRemove.length; i++) {
                            var index = i;
                            var msg = notify.addInfoMessage("Gemmer...", false);
                            $http.delete($scope.autoSaveUrl + "?organizationId=" + user.currentOrganizationId + "&elemId=" + toRemove[index].id)
                                .then(function onSuccess(result) {
                                    _.remove(contract.agreementElements, (element) => { return element.id === toRemove[index].id });
                                    msg.toSuccessMessage("Feltet er opdateret.");
                                }, function onError(error) {
                                    msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                });
                            
                        }
                    }
                }
            });

            const mapDataToViewmodelArray = (systemRelations: [Kitos.Models.Api.ItSystemUsage.Relation.IItSystemUsageRelationDTO]) => {
                return Kitos.Models.ViewModel.ItSystemUsage.Relation.SystemRelationMapper.mapSystemRelationsToViewModels(
                    systemRelations,
                    Kitos.Configs.RelationTableCellParagraphSizeConfig.maxTextFieldCharCount,
                    Kitos.Configs.RelationTableCellParagraphSizeConfig.shortTextLineCount);
            }

            systemRelationService.getRelationWithContract(contract.id)
                .then((systemRelations: [Kitos.Models.Api.ItSystemUsage.Relation.IItSystemUsageRelationDTO]) => {
                    $scope.usageRelations = mapDataToViewmodelArray(systemRelations);
                });

            function formatAssociatedSystems(associatedSystemUsages: any);
            function formatAssociatedSystems(associatedSystemUsages) {

                //helper functions
                function deleteAssociatedSystem(associatedSystem: any);
                function deleteAssociatedSystem(associatedSystem) {
                    return $http.delete("api/itcontract/" + contract.id + "?systemUsageId=" + associatedSystem.id + "&organizationId=" + user.currentOrganizationId);
                }

                function postAssociatedSystem(associatedSystem: any);
                function postAssociatedSystem(associatedSystem) {
                    return $http.post("api/itcontract/" + contract.id + "?systemUsageId=" + associatedSystem.selectedSystem.id + "&organizationId=" + user.currentOrganizationId);
                }

                //for each row of associated system
                _.each(associatedSystemUsages, (systemUsage: { show; delete; id; }) => {

                    systemUsage.show = true;

                    //delete the row
                    systemUsage.delete = () => {
                        deleteAssociatedSystem(systemUsage)
                            .then(function onSuccess(result) {
                                notify.addSuccessMessage("Rækken er slettet.");
                                _.remove(contract.associatedSystemUsages, { id: systemUsage.id });
                                systemUsage.show = false;
                            }, function onError(result) {
                                notify.addErrorMessage("Kunne ikke slette rækken");
                            });
                    };

                });

                $scope.associatedSystemUsages = associatedSystemUsages;

                $scope.newAssociatedSystemUsage = {
                    save() {
                        //post new binding
                        postAssociatedSystem($scope.newAssociatedSystemUsage)
                            .then(function onSuccess(result) {

                                notify.addSuccessMessage("Rækken er tilføjet.");

                                //then reformat and redraw the rows
                                contract.associatedSystemUsages = result.data.response;
                                formatAssociatedSystems(result.data.response);

                            }, function onError(result) {

                                //couldn't add new binding
                                notify.addErrorMessage("Fejl! Kunne ikke tilføje rækken!");

                            });
                    }
                };
            }


            formatAssociatedSystems(contract.associatedSystemUsages);

            $scope.expandParagraph = (e) => {
                Kitos.Utility.TableManipulation.expandRetractParagraphCell(e, Kitos.Configs.RelationTableCellParagraphSizeConfig.shortTextLineCount);
            };

            $scope.itSystemUsagesSelectOptions = {
                minimumInputLength: 1,
                initSelection(elem, callback) {
                },
                ajax: {
                    data(term, page) {
                        return { query: term };
                    },
                    quietMillis: 500,
                    transport(queryParams) {
                        var res = $http.get("api/itSystemUsage?organizationId=" + user.currentOrganizationId + "&q=" + queryParams.data.query + "&take=25").then(queryParams.success);
                        res.abort = () => null;

                        return res;
                    },

                    results(data, page) {
                        var results = [];

                        // for each system usages
                        _.each(data.data.response, (usage: { id; itSystemName; itSystemDisabled; }) => {
                            if (!usage.itSystemDisabled) {
                                results.push({
                                    // the id of the system usage id, that is selected
                                    id: usage.id,
                                    // name of the system is the label for the select2
                                    text: $filter('limitToDots')(usage.itSystemName, 30),
                                });
                            }
                        });

                        return { results: results };
                    }
                }
            };

        }
    ]);
})(angular, app);
