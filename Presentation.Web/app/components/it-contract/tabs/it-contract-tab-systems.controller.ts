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
                    exhibitedInterfaces: [
                        "$http", "contract", function ($http, contract) {
                            return $http.get("api/ItInterfaceExhibitUsage/?contractId=" + contract.id).then(function (result) {
                                return result.data.response;
                            });
                        }
                    ],
                    usedInterfaces: [
                        "$http", "contract", function ($http, contract) {
                            return $http.get("api/ItInterfaceUsage/?contractId=" + contract.id).then(function (result) {
                                return result.data.response;
                            });
                        }
                    ],
                    interfaces: [
                        "$http", function ($http) {
                            return $http.get("odata/ItInterfaces").then(function (result) {
                                return result.data.value;
                            });
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
        "$scope", "$http", "$state", "$stateParams", "notify", "user", "contract", "exhibitedInterfaces", "usedInterfaces", "agreementElements", "_", "$filter",
        function ($scope, $http, $state, $stateParams, notify, user, contract, exhibitedInterfaces, usedInterfaces, agreementElements, _, $filter) {

            $scope.autoSaveUrl = "api/itcontract/" + $stateParams.id;
            $scope.exhibitedInterfaces = exhibitedInterfaces;
            $scope.usedInterfaces = usedInterfaces;
            $scope.contract = contract;

            $scope.agreementElements = agreementElements;
            $scope.selectedAgreementElementIds = _.map(contract.agreementElements, "id");
            $scope.selectedAgreementElementNames = _.map(contract.agreementElements, "name");

            $scope.deleteExhibit = function (exhibitId, usageId) {
                $http({
                    method: "DELETE",
                    url: "api/itInterfaceExhibitUsage/?usageId=" + usageId + "&exhibitId=" + exhibitId
                })
                    .success(function () {
                        notify.addSuccessMessage("Snitfladerelationen er slettet.");
                        reload();
                    })
                    .error(function () {
                        notify.addSuccessMessage("Fejl! Snitfladerelationen kunne ikke slettes.");
                    });
            }

            $scope.deleteUsed = function (usageId, sysId, interfaceId) {
                $http({
                    method: "DELETE",
                    url: "api/ItInterfaceUsage/?usageId=" + usageId + "&sysId=" + sysId + "&interfaceId=" + interfaceId + "&organizationId=" + user.currentOrganizationId
                })
                    .success(function () {
                        notify.addSuccessMessage("Snitfladerelationen er slettet.");
                        reload();
                    })
                    .error(function () {
                        notify.addSuccessMessage("Fejl! Snitfladerelationen kunne ikke slettes.");
                    });
            }

            function reload() {
                $state.go(".", null, { reload: true });
            }


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

            $scope.newAssociatedInterfaceSave = function () {
                var url = "";
                if ($scope.newAssociatedInterfaceRelation == "exhibit")
                    url = "api/itInterfaceExhibitUsage?usageId=" + $scope.newAssociatedInterfaceSelectedSystemUsage.id + "&exhibitId=" + $scope.newAssociatedInterfaceSelectedInterfaceUsage.id;

                if ($scope.newAssociatedInterfaceRelation == "using")
                    url = "api/ItInterfaceUsage?usageId=" + $scope.newAssociatedInterfaceSelectedSystemUsage.id
                        + "&sysId=" + $scope.newAssociatedInterfaceSelectedInterfaceUsage.id.sysId
                        + "&interfaceId=" + $scope.newAssociatedInterfaceSelectedInterfaceUsage.id.intfId
                        + "&organizationId=" + user.currentOrganizationId;

                $http({
                    method: "PATCH",
                    url: url,
                    data: {
                        itContractId: contract.id
                    }
                })
                    .success(function () {
                        reload();
                    });
            }

            // select2 options for looking up a system's interfaces
            $scope.itInterfaceUsagesSelectOptions = {
                minimumInputLength: 1,
                initSelection: function (elem, callback) {
                },
                ajax: {
                    data: function (term, page) {
                        return { query: term };
                    },
                    quietMillis: 500,
                    transport: function (queryParams) {
                        var url = "";
                        if ($scope.newAssociatedInterfaceRelation == "exhibit")
                            url = "api/exhibit?sysId=" + $scope.newAssociatedInterfaceSelectedSystemUsage.itSystemId + "&orgId=" + user.currentOrganizationId + "&q=" + queryParams.data.query;

                        if ($scope.newAssociatedInterfaceRelation == "using")
                            url = `odata/ItInterfaces?$filter=contains(Name, '${queryParams.data.query}')`;

                        var res = $http.get(url).then(queryParams.success);
                        res.abort = function () {
                            return null;
                        };

                        return res;
                    },

                    results: function (data, page) {
                        var results = [];

                        // for each interface usages
                        if ($scope.newAssociatedInterfaceRelation == "exhibit") {
                            // interface exhibit
                            _.each(data.data.response, function (usage: { id; itInterfaceName; itInterfaceDisabled; }) {
                                if (!usage.itInterfaceDisabled) {
                                    results.push({
                                        // use the id of the interface usage
                                        id: usage.id,
                                        // use the name of the actual interface
                                        text: $filter('limitToDots')(usage.itInterfaceName, 30) 
                                    });
                                }
                            });
                        } else {
                            // interface usage
                            _.each(data.data.value, function (usage: { Id; Name; Disabled; }) {
                                if (!usage.Disabled) {
                                    results.push({
                                        // use the id of the interface usage
                                        id: { intfId: usage.Id, sysId: $scope.newAssociatedInterfaceSelectedSystemUsage.itSystemId },
                                        // use the name of the actual interface
                                        text: $filter('limitToDots')(usage.Name, 30)
                                    });
                                }

                            });
                        }

                        return { results: results };
                    }
                }
            };

            // select2 options for looking up it system usages
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
