(function (ng, app) {
    app.config([
        "$stateProvider", function ($stateProvider) {
            $stateProvider.state("it-contract.edit.systems", {
                url: "/systems",
                templateUrl: "app/components/it-contract/tabs/it-contract-tab-systems.view.html",
                controller: "contract.EditSystemsCtrl",
                resolve: {
                    user: [
                        "userService", function (userService) {
                            return userService.getUser();
                        }
                    ],
                    agreementElements: [
                        "$http", function ($http) {
                            return $http.get("odata/LocalAgreementElementTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(function (result) {
                                return result.data.value;
                            });
                        }
                    ]
                }
            });
        }
    ]);

    app.controller("contract.EditSystemsCtrl", [
        "$scope", "$http", "$stateParams", "notify", "user", "contract", "agreementElements", "_", "$filter", "systemRelationService",
        function ($scope, $http, $stateParams, notify, user, contract, agreementElements, _, $filter, systemRelationService) {

            const maxTextFieldCharCount = 199;
            const shortTextLineCount = 4;

            $scope.autoSaveUrl = "api/itcontract/" + $stateParams.id;
            $scope.contract = contract;

            $scope.agreementElements = agreementElements;
            $scope.selectedAgreementElementIds = _.map(contract.agreementElements, "id");
            $scope.selectedAgreementElementNames = _.map(contract.agreementElements, "name");

            const mapDataToViewmodelArray = (systemRelations: [Kitos.Models.ItSystemUsage.Relation.IItSystemUsageRelationDTO]) => {
                return Kitos.Models.ItSystemUsage.Relation.SystemRelationMapper.mapSystemRelationsToViewModels(systemRelations,maxTextFieldCharCount,shortTextLineCount);
            }

            systemRelationService.getRelationWithContract(contract.id)
                .then((systemRelations: [Kitos.Models.ItSystemUsage.Relation.IItSystemUsageRelationDTO]) => {
                    $scope.usageRelations = mapDataToViewmodelArray(systemRelations);
                });

            systemRelationService.getRelationWithContract(contract.id)
                .then((systemRelations: [Kitos.Models.ItSystemUsage.Relation.IItSystemUsageRelationDTO]) => {
                    $scope.usageRelations = mapDataToViewmodelArray(systemRelations);
                });

            function formatAssociatedSystems(associatedSystemUsages) {

                //helper functions
                function deleteAssociatedSystem(associatedSystem) {
                    return $http.delete("api/itcontract/" + contract.id + "?systemUsageId=" + associatedSystem.id + "&organizationId=" + user.currentOrganizationId);
                }

                function postAssociatedSystem(associatedSystem) {
                    return $http.post("api/itcontract/" + contract.id + "?systemUsageId=" + associatedSystem.selectedSystem.id + "&organizationId=" + user.currentOrganizationId);
                }

                //for each row of associated system
                _.each(associatedSystemUsages, function (systemUsage: { show; delete; id;}) {

                    systemUsage.show = true;

                    //delete the row
                    systemUsage.delete = function () {
                        deleteAssociatedSystem(systemUsage).success(function () {
                            notify.addSuccessMessage("Rækken er slettet.");
                            _.remove(contract.associatedSystemUsages, { id: systemUsage.id });
                            systemUsage.show = false;
                        }).error(function () {
                            notify.addErrorMessage("Kunne ikke slette rækken");
                        });
                    };

                });

                $scope.associatedSystemUsages = associatedSystemUsages;

                $scope.newAssociatedSystemUsage = {
                    save: function () {
                        //post new binding
                        postAssociatedSystem($scope.newAssociatedSystemUsage).success(function (result) {

                            notify.addSuccessMessage("Rækken er tilføjet.");

                            //then reformat and redraw the rows
                            contract.associatedSystemUsages = result.response;
                            formatAssociatedSystems(result.response);

                        }).error(function (result) {

                            //couldn't add new binding
                            notify.addErrorMessage("Fejl! Kunne ikke tilføje rækken!");

                        });
                    }
                };
            }


            formatAssociatedSystems(contract.associatedSystemUsages);

            $scope.expandParagraph = (e) => {
                Kitos.Utility.TableManipulation.expandRetractParagraphCell(e, shortTextLineCount);
            };

            $scope.itSystemUsagesSelectOptions = {
                minimumInputLength: 1,
                initSelection: function (elem, callback) {
                },
                ajax: {
                    data: function (term, page) {
                        return { query: term };
                    },
                    quietMillis: 500,
                    transport: function (queryParams) {
                        var res = $http.get("api/itSystemUsage?organizationId=" + user.currentOrganizationId + "&q=" + queryParams.data.query).then(queryParams.success);
                        res.abort = function () {
                            return null;
                        };

                        return res;
                    },

                    results: function (data, page) {
                        var results = [];

                        // for each system usages
                        _.each(data.data.response, function (usage: { id; itSystem; }) {
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
