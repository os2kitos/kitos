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
                        "$http", $http => $http.get("odata/LocalAgreementElementTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(result => result.data.value)
                    ]
                }
            });
        }
    ]);

    app.controller("contract.EditSystemsCtrl", [
        "$scope", "$http", "$stateParams", "notify", "user", "contract", "agreementElements", "_", "$filter", "systemRelationService",
        ($scope, $http, $stateParams, notify, user, contract, agreementElements, _, $filter, systemRelationService) => {

            $scope.autoSaveUrl = `api/itcontract/${$stateParams.id}`;
            $scope.contract = contract;

            $scope.agreementElements = agreementElements;
            $scope.selectedAgreementElementIds = _.map(contract.agreementElements, "id");
            $scope.selectedAgreementElementNames = _.map(contract.agreementElements, "name");

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
                        deleteAssociatedSystem(systemUsage).success(() => {
                            notify.addSuccessMessage("Rækken er slettet.");
                            _.remove(contract.associatedSystemUsages, { id: systemUsage.id });
                            systemUsage.show = false;
                        }).error(() => {
                            notify.addErrorMessage("Kunne ikke slette rækken");
                        });
                    };

                });

                $scope.associatedSystemUsages = associatedSystemUsages;

                $scope.newAssociatedSystemUsage = {
                    save() {
                        //post new binding
                        postAssociatedSystem($scope.newAssociatedSystemUsage).success(result => {

                            notify.addSuccessMessage("Rækken er tilføjet.");

                            //then reformat and redraw the rows
                            contract.associatedSystemUsages = result.response;
                            formatAssociatedSystems(result.response);

                        }).error(result => {

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
                        _.each(data.data.response, (usage: { id; itSystem; }) => {
                            if (!usage.itSystem.disabled) {
                                results.push({
                                    // the id of the system usage id, that is selected
                                    id: usage.id,
                                    // name of the system is the label for the select2
                                    text: $filter('limitToDots')(usage.itSystem.name, 30),
                                    // the if the system id that is selected
                                    itSystemId: usage.itSystem.id
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
