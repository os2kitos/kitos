(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-project.edit.org", {
            url: "/org",
            templateUrl: "app/components/it-project/tabs/it-project-tab-org.view.html",
            controller: "project.EditOrgCtrl",
            resolve: {
                // re-resolve data from parent cause changes here wont cascade to it
                project: ["$http", "$stateParams", function ($http, $stateParams) {
                    return $http.get("api/itproject/" + $stateParams.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                isTransversal: ["project", function (project) {
                    return project.isTransversal;
                }],
                selectedOrgUnits: ["$http", "$stateParams", function ($http, $stateParams) {
                    var projectId = $stateParams.id;
                    return $http.get("api/itProjectOrgUnitUsage/" + projectId)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                responsibleOrgUnitId: ["$http", "$stateParams", function ($http, $stateParams) {
                    var projectId = $stateParams.id;
                    return $http.get("api/itProjectOrgUnitUsage/" + projectId + "?responsible=true")
                        .then(function (result) {
                            if (result.data.response)
                                return result.data.response.id;
                            return null;
                        }
                        );
                }],
                orgUnitsTree: ["$http", "project", function ($http, project) {
                    return $http.get("api/organizationunit/?organization=" + project.organizationId)
                        .then(function (result) {
                            return [result.data.response]; // to array for ngRepeat to work
                        });
                }]
            }
        });
    }]);

    app.controller("project.EditOrgCtrl",
        ["$scope", "$http", "$stateParams", "notify", "isTransversal", "orgUnitsTree", "selectedOrgUnits", "responsibleOrgUnitId", "user", "entityMapper", 
            function ($scope, $http, $stateParams, notify, isTransversal, orgUnitsTree, selectedOrgUnits, responsibleOrgUnitId, user, entityMapper) {
                $scope.orgUnitsTree = orgUnitsTree;
                $scope.isTransversal = isTransversal;
                $scope.selectedOrgUnits = entityMapper.mapApiResponseToSelect2ViewModel(selectedOrgUnits);
                $scope.responsibleOrgUnit = _.find($scope.selectedOrgUnits, (orgUnit) => orgUnit.id === responsibleOrgUnitId);
                var projectId = $stateParams.id;
                $scope.patchUrl = "api/itproject/" + projectId;

                $scope.saveResponsible = function (orgUnitId) {
                    if (orgUnitId != null) {
                        var msg = notify.addInfoMessage("Gemmer... ");
                        $http.post("api/itProjectOrgUnitUsage/?projectId=" + projectId + "&orgUnitId=" + orgUnitId + "&responsible")
                            .then(function onSuccess(result) {
                                msg.toSuccessMessage("Gemt!");
                            }, function onError(result) {
                                msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                            });
                    } else {
                        var msg = notify.addInfoMessage("Gemmer... ");
                        $http.delete("api/itProjectOrgUnitUsage/?projectId=" + projectId + "&responsible")
                            .then(function onSuccess(result) {
                                msg.toSuccessMessage("Gemt!");
                            }, function onError(result) {
                                msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                            });
                    }
                };

                $scope.save = function (obj) {
                    var msg = notify.addInfoMessage("Gemmer... ");
                    if (obj.selected) {
                        $http.post("api/itproject/" + projectId + "?organizationunit=" + obj.id + "&organizationId=" + user.currentOrganizationId)
                            .then(function onSuccess(result) {
                                msg.toSuccessMessage("Gemt!");
                                $scope.selectedOrgUnits.push({ id: obj.id, text: obj.name });
                            }, function onError(result) {
                                msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                            });
                    } else {
                        $http.delete("api/itproject/" + projectId + "?organizationunit=" + obj.id + "&organizationId=" + user.currentOrganizationId)
                            .then(function onSuccess(result) {
                                msg.toSuccessMessage("Gemt!");

                                var indexOf;
                                // find the index of the orgunit
                                var found = _.filter($scope.selectedOrgUnits, function (element: { id }, index) {
                                    var equal = element.id == obj.id;
                                    // set outer scope indexOf, to be used later
                                    if (equal) indexOf = index;
                                    return equal;
                                });
                                // remove orgunit from the responsible dropdown
                                if (found) $scope.selectedOrgUnits.splice(indexOf, 1);

                                // if responsible is the orgunit being removed unselect it from the dropdown
                                if (obj.id == $scope.responsibleOrgUnitId)
                                    $scope.responsibleOrgUnitId = "";
                            }, function onError(result) {
                                msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                            });
                    }
                };

                function searchTree(element, matchingId) {
                    if (element.id == matchingId) {
                        return element;
                    } else if (element.children != null) {
                        var result = null;
                        for (var i = 0; result == null && i < element.children.length; i++) {
                            result = searchTree(element.children[i], matchingId);
                        }
                        return result;
                    }
                    return null;
                }

                var selectedOrgUnitIds = _.map(selectedOrgUnits, "id");
                _.each(selectedOrgUnitIds, function (id) {
                    var found = searchTree(orgUnitsTree[0], id);
                    if (found) {
                        found.selected = true;
                    }
                });
            }
        ]
    );
})(angular, app);
