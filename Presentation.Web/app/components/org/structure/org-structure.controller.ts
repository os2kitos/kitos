(function (ng, app) {
    app.config([
        "$stateProvider", function ($stateProvider) {
            $stateProvider.state("organization.structure", {
                url: "/structure",
                templateUrl: "app/components/org/structure/org-structure.view.html",
                controller: "org.StructureCtrl",
                resolve: {
                    orgUnits: [
                        "$http", "user", function ($http: ng.IHttpService, user) {
                            return $http.get<Kitos.API.Models.IApiWrapper<any>>("api/organizationunit?organization=" + user.currentOrganizationId).then((result) => {
                                return result.data.response;
                            });
                        }
                    ],
                    localOrgUnitRoles: ['$http', function ($http) {
                        return $http.get("odata/LocalOrganizationUnitRoles?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                            .then(function (result) {
                                return result.data.value;
                            });
                    }],
                    orgUnitRoles: ['$http', function ($http) {
                        return $http.get("odata/OrganizationUnitRoles")
                            .then(function (result) {
                                return result.data.value;
                            });
                    }],
                    user: [
                        "userService", function (userService) {
                            return userService.getUser();
                        }
                    ],
                    hasWriteAccess: [
                        '$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                            return $http.get('api/Organization/' + user.currentOrganizationId + "?hasWriteAccess=true&organizationId=" + user.currentOrganizationId)
                                .then(function (result) {
                                    return result.data.response;
                                });
                        }
                    ]
                }
            });
        }
    ]);

    app.controller("org.StructureCtrl", [
        "$scope", "$http", "$q", "$filter", "$uibModal", "$state", "notify", "orgUnits", "localOrgUnitRoles", "orgUnitRoles", "user", "hasWriteAccess",
        function ($scope, $http: ng.IHttpService, $q, $filter, $modal, $state, notify, orgUnits, localOrgUnitRoles, orgUnitRoles, user, hasWriteAccess) {
            $scope.orgId = user.currentOrganizationId;
            $scope.pagination = {
                skip: 0,
                take: 50
            };
            $scope.rightsPagination = {
                skip: 0,
                take: 15
            };

            //cache
            var orgs = [];

            //flattened map of all loaded orgUnits
            $scope.orgUnits = {};
            $scope.hasWriteAccess = hasWriteAccess;

            $scope.orgUnitRoles = orgUnitRoles;
            $scope.activeOrgRoles = localOrgUnitRoles;
            $scope.orgRoles = {};
            _.each(localOrgUnitRoles, function (orgRole: { Id }) {
                $scope.orgRoles[orgRole.Id] = orgRole;
            });


            function flattenAndSave(orgUnit, inheritWriteAccess, parentOrgunit) {
                orgUnit.parent = parentOrgunit;

                //restore previously saved settings
                if ($scope.orgUnits[orgUnit.id]) {
                    var old = $scope.orgUnits[orgUnit.id];
                    orgUnit.isOpen = old.isOpen;
                }

                checkForDefaultUnit(orgUnit);

                $scope.orgUnits[orgUnit.id] = orgUnit;

                if (!inheritWriteAccess) {
                    $http.get<Kitos.API.Models.IApiWrapper<any>>("api/organizationUnit/" + orgUnit.id + "?hasWriteAccess&organizationId=" + user.currentOrganizationId).then((result) => {
                        orgUnit.hasWriteAccess = result.data.response;

                        _.each(orgUnit.children, function (u) {
                            flattenAndSave(u, result.data.response, orgUnit);
                        });

                    });
                } else {
                    orgUnit.hasWriteAccess = true;

                    _.each(orgUnit.children, function (u) {
                        return flattenAndSave(u, true, orgUnit);
                    });
                }
            }

            function checkForDefaultUnit(unit) {
                if (!user.currentOrganizationUnitId) return;

                if (!unit || unit.id !== user.currentOrganizationUnitId) return;

                open(unit);
                $scope.chooseOrgUnit(unit);

                function open(u) {
                    u.isOpen = true;
                    if (u.parent) open(u.parent);
                }
            }

            function loadUnits() {
                var rootNode = orgUnits;
                $scope.nodes = [rootNode];

                flattenAndSave(rootNode, false, null);
            }

            $scope.showChildren = false;

            $scope.chosenOrgUnit = null;

            $scope.chooseOrgUnit = function (node, event) {
                if (event) {
                    var isDiv = angular.element(event.target)[0].tagName === "DIV";
                    if (!isDiv) {
                        return;
                    }
                }
                if ($scope.chosenOrgUnit === node) return;

                // reset state between selecting the current organization
                $scope.showChildren = false;

                if ($scope.chosenOrgUnit === node) return;

                //get organization related to the org unit
                if (!node.organization) {
                    //try get from cache
                    if (orgs[node.organizationId]) {
                        node.organization = orgs[node.organizationId];
                    } else {
                        //else get from server
                        $http.get<Kitos.API.Models.IApiWrapper<any>>("api/organization/" + node.organizationId).then((result) => {
                            node.organization = result.data.response;

                            //save to cache
                            orgs[node.organizationId] = result.data.response;
                        });
                    }
                }

                // reset pagination
                $scope.rightsPagination = {
                    skip: 0,
                    take: 15
                };

                loadRights(node);
                loadTasks();
            };

            function loadRights(node) {
                //get org rights on the org unit and subtree
                $http.get<Kitos.API.Models.IApiWrapper<any>>("api/organizationUnitRight/" + node.id + "?paged&take=" + $scope.rightsPagination.take + "&skip=" + $scope.rightsPagination.skip).then((result) => {
                    var paginationHeader = JSON.parse(result.headers("X-Pagination"));
                    $scope.totalRightsCountCopy = paginationHeader.TotalCount;
                    node.orgRights = result.data.response;

                    var count = 0;
                    _.each(node.orgRights, function (right: { userForSelect; roleForSelect; user; roleId; show; objectId; }) {
                        right.userForSelect = { id: right.user.id, text: right.user.fullName };
                        right.roleForSelect = right.roleId;
                        right.show = $scope.showChildren || belongsToChosenNode(node, right);
                        if (right.show)
                            count++;
                    });
                    
                    $scope.totalRightsCount = ($scope.showChildren) ? $scope.totalRightsCountCopy : count;
                });

                $scope.chosenOrgUnit = node;
            }

            // don't show users in subunits if showChildren is false
            $scope.toggleChildren = function () {
                const node = $scope.chosenOrgUnit;
                var count = 0;
                _.each(node.orgRights,
                    function (right: { show; }) {
                        right.show = $scope.showChildren || belongsToChosenNode(node, right);
                        if (right.show)
                            count++;
                    });

                $scope.totalRightsCount = ($scope.showChildren) ? $scope.totalRightsCountCopy : count;

                if (!$scope.showChildren && $scope.totalRightsCount === 0 && $scope.rightsPagination.skip > 0) {
                    $scope.rightsPagination.skip = 0;
                    loadRights($scope.chosenOrgUnit);
                }
            };

            function belongsToChosenNode(node, right) {
                return node.id === right.objectId;
            }

            $scope.$watch("selectedUser", function () {
                $scope.submitRight();
            });

            /* the role of "medarbejder" */
            function getDefaultNewRole() {
                return 3;
            }

            $scope.newRole = getDefaultNewRole();

            $scope.submitRight = function () {
                if (!$scope.selectedUser || !$scope.newRole) return;

                var oId = $scope.chosenOrgUnit.id;
                var rId = parseInt($scope.newRole);
                var uId = $scope.selectedUser.id;

                if (!oId || !rId || !uId) return;

                var data = {
                    "roleId": rId,
                    "userId": uId
                };

                $http.post<Kitos.API.Models.IApiWrapper<any>>("api/organizationUnitRight/" + oId + "?organizationId=" + user.currentOrganizationId, data).then((result) => {
                    notify.addSuccessMessage(result.data.response.user.fullName + " er knyttet i rollen");

                    $scope.chosenOrgUnit.orgRights.push({
                        "objectId": result.data.response.objectId,
                        "roleId": result.data.response.roleId,
                        "userId": result.data.response.userId,
                        "user": result.data.response.user,
                        "userForSelect": { id: result.data.response.userId, text: result.data.response.user.fullName },
                        "roleForSelect": result.data.response.roleId,
                        show: true
                    });

                    $scope.newRole = getDefaultNewRole();
                    $scope.selectedUser = "";
                }, (error) => {
                    notify.addErrorMessage("Fejl!");
                });
            };

            $scope.deleteRight = function (right) {
                var oId = right.objectId;
                var rId = right.roleId;
                var uId = right.userId;

                $http.delete<Kitos.API.Models.IApiWrapper<any>>("api/organizationUnitRight/" + oId + "?rId=" + rId + "&uId=" + uId + "&organizationId=" + user.currentOrganizationId).then((deleteResult) => {
                    right.show = false;
                    notify.addSuccessMessage("Rollen er slettet!");
                }, (error) => {
                    notify.addErrorMessage("Kunne ikke slette rollen!");
                });
            };

            $scope.updateRight = function (right) {
                if (!right.roleForSelect || !right.userForSelect) return;

                if (!$scope.checkIfRoleIsAvailable(right.roleForSelect)) {
                    right.edit = false;
                    return;
                }

                //old values
                var oIdOld = right.objectId;
                var rIdOld = right.roleId;
                var uIdOld = right.userId;

                //new values
                var oIdNew = right.objectId;
                var rIdNew = right.roleForSelect;
                var uIdNew = right.userForSelect.id;

                //if nothing was changed, just exit edit-mode
                if (oIdOld === oIdNew && rIdOld === rIdNew && uIdOld === uIdNew) {
                    right.edit = false;
                }

                //otherwise, we should delete the old entry, then add a new one

                $http.delete<Kitos.API.Models.IApiWrapper<any>>("api/organizationUnitRight/" + oIdOld + "?rId=" + rIdOld + "&uId=" + uIdOld + "&organizationId=" + user.currentOrganizationId).then((deleteResult) => {
                    var data = {
                        "roleId": rIdNew,
                        "userId": uIdNew
                    };

                    $http.post<Kitos.API.Models.IApiWrapper<any>>("api/organizationUnitRight/" + oIdNew + "?organizationId=" + user.currentOrganizationId, data).then((result) => {
                        right.roleId = result.data.response.roleId;
                        right.user = result.data.response.user;
                        right.userId = result.data.response.userId;

                        right.edit = false;

                        notify.addSuccessMessage(right.user.fullName + " er knyttet i rollen");
                    }, (error) => {
                        // we successfully deleted the old entry, but didn't add a new one
                        right.show = false;

                        notify.addErrorMessage("Fejl!");
                    });
                }, (error) => {
                    // couldn't delete the old entry, just reset select options
                    right.userForSelect = { id: right.user.id, text: right.user.fullName };
                    right.roleForSelect = right.roleId;

                    notify.addErrorMessage("Fejl!");
                });
            };

            $scope.rightSortBy = "orgUnitName";
            $scope.rightSortReverse = false;
            $scope.rightSort = function (right) {
                switch ($scope.rightSortBy) {
                    case "orgUnitName":
                        return $scope.orgUnits[right.objectId].name;
                    case "roleName":
                        return $scope.orgRoles[right.roleId].Priority;
                    case "userName":
                        return right.user.name;
                    case "userEmail":
                        return right.user.email;
                    default:
                        return $scope.orgUnits[right.objectId].name;
                }
            };

            $scope.rightSortChange = function (val) {
                if ($scope.rightSortBy === val) {
                    $scope.rightSortReverse = !$scope.rightSortReverse;
                } else {
                    $scope.rightSortReverse = false;
                }

                $scope.rightSortBy = val;
            };

            $scope.editUnit = function (unit) {
                var modal = $modal.open({
                    templateUrl: "app/components/org/structure/org-structure-modal-edit.view.html",
                    controller: [
                        "$scope", "$uibModalInstance", "autofocus", function ($modalScope, $modalInstance, autofocus) {
                            autofocus();

                            // edit or create-new mode
                            $modalScope.isNew = false;

                            // holds a list of org units, which the user can select as the parent
                            $modalScope.orgUnits = [];

                            // filter out those orgunits, that are outside the organisation
                            // or is currently a subdepartment of the unit
                            function filter(node) {
                                if (node.organizationId !== unit.organizationId) return;

                                // this avoid every subdepartment
                                if (node.id === unit.id) return;

                                $modalScope.orgUnits.push(node);

                                _.each(node.children, filter);
                            }

                            _.each($scope.nodes, filter);

                            // format the selected unit for editing
                            $modalScope.orgUnit = {
                                'id': unit.id,
                                'oldName': unit.name,
                                'newName': unit.name,
                                'newEan': unit.ean,
                                'localId': unit.localId,
                                'newParent': unit.parentId,
                                'orgId': unit.organizationId,
                                'isRoot': unit.parentId == undefined
                            };

                            // only allow changing the parent if user is admin, and the unit isn't at the root
                            $modalScope.isAdmin = user.isGlobalAdmin || user.isLocalAdmin;
                            $modalScope.canChangeParent = $modalScope.isAdmin && !$modalScope.orgUnit.isRoot;

                            $modalScope.patch = function () {
                                // don't allow duplicate submitting
                                if ($modalScope.submitting) return;

                                var name = $modalScope.orgUnit.newName;
                                var parent = $modalScope.orgUnit.newParent;
                                var ean = $modalScope.orgUnit.newEan;
                                var localId = $modalScope.orgUnit.localId;

                                if (!name) return;

                                var data = {
                                    "name": name,
                                    "ean": ean,
                                    "localId": localId
                                };

                                // only allow changing the parent if user is admin, and the unit isn't at the root
                                if ($modalScope.canChangeParent && parent) data["parentId"] = parent;

                                $modalScope.submitting = true;

                                var id = unit.id;

                                $http<Kitos.API.Models.IApiWrapper<any>>({ method: "PATCH", url: "api/organizationUnit/" + id + "?organizationId=" + user.currentOrganizationId, data: data }).then((result) => {
                                    notify.addSuccessMessage(name + " er ændret.");

                                    $modalInstance.close(result.data.response);
                                }, (error: ng.IHttpPromiseCallbackArg<Kitos.API.Models.IApiWrapper<any>>) => {
                                    $modalScope.submitting = false;
                                    if (error.data.msg.indexOf("Duplicate entry") > -1) {
                                        notify.addErrorMessage("Fejl! Enhed ID er allerede brugt!");
                                    } else {
                                        notify.addErrorMessage("Fejl! " + name + " kunne ikke ændres!");
                                    }
                                });
                            };

                            $modalScope.post = function () {
                                // don't allow duplicate submitting
                                if ($modalScope.submitting) return;

                                var name = $modalScope.newOrgUnit.name;
                                if (!name) return;

                                var parent = $modalScope.newOrgUnit.parent;
                                var orgId = $modalScope.newOrgUnit.orgId;
                                var ean = $modalScope.newOrgUnit.ean;
                                var localId = $modalScope.newOrgUnit.localId;

                                var data = {
                                    "name": name,
                                    "parentId": parent,
                                    "organizationId": orgId,
                                    "ean": ean,
                                    "localId": localId
                                };

                                $modalScope.submitting = true;

                                $http<Kitos.API.Models.IApiWrapper<any>>({ method: "POST", url: "api/organizationUnit/", data: data }).then((result) => {
                                    notify.addSuccessMessage(name + " er gemt.");

                                    $modalInstance.close(result.data.response);
                                }, (error: ng.IHttpPromiseCallbackArg<Kitos.API.Models.IApiWrapper<any>>) => {
                                    $modalScope.submitting = false;
                                    if (error.data.msg.indexOf("Duplicate entry") > -1) {
                                        notify.addErrorMessage("Fejl! Enhed ID er allerede brugt!");
                                    } else {
                                        notify.addErrorMessage("Fejl! " + name + " kunne ikke oprettes!");
                                    }
                                });
                            };

                            $modalScope.new = function () {
                                autofocus();

                                $modalScope.createNew = true;
                                $modalScope.newOrgUnit = {
                                    name: "",
                                    parent: $modalScope.orgUnit.id,
                                    orgId: $modalScope.orgUnit.orgId
                                };
                            };

                            $modalScope.delete = function () {
                                //don't allow duplicate submitting
                                if ($modalScope.submitting) return;

                                $modalScope.submitting = true;

                                $http.delete<Kitos.API.Models.IApiWrapper<any>>("api/organizationUnit/" + unit.id + "?organizationId=" + user.currentOrganizationId).then((result) => {
                                    $modalInstance.close();
                                    notify.addSuccessMessage(unit.name + " er slettet!");
                                }, (error) => {
                                    $modalScope.submitting = false;

                                    notify.addErrorMessage(`Fejl! ${unit.name} kunne ikke slettes!<br /><br />
                                                            Organisationsenheden bliver brugt som reference i en eller flere IT Projekter, IT Systemer og/eller IT Kontrakter.<br /><br />
                                                            Fjern referencen for at kunne slette denne enhed.`);
                                });

                            };

                            $modalScope.cancel = function () {
                                $modalInstance.dismiss("cancel");
                            };
                        }
                    ]
                });

                modal.result.then(function (returnedUnit) {
                    $state.go($state.current, {}, { reload: true });
                    loadUnits();
                });
            };

            $scope.$watch("selectedTaskGroup", function (newVal, oldVal) {
                $scope.pagination.skip = 0;
                loadTasks();
            });

            $scope.$watchCollection("pagination", loadTasks);
            $scope.$watchCollection("rightsPagination", function () {
                loadRights($scope.chosenOrgUnit);
            });

            // default kle mode
            $scope.showAllTasks = true;
            // default kle sort order
            $scope.pagination.orderBy = "taskKey";

            // change between show all tasks and only show active tasks
            $scope.changeTaskView = function () {
                $scope.showAllTasks = !$scope.showAllTasks;
                $scope.pagination.orderBy = $scope.showAllTasks ? "taskKey" : "taskRef.taskKey";
                $scope.pagination.skip = 0;
                loadTasks();
            };

            function loadTasks() {
                if (!$scope.chosenOrgUnit) return;

                var url = "api/organizationUnit/" + $scope.chosenOrgUnit.id;

                if ($scope.showAllTasks) url += "?tasks";
                else url += "?usages";

                url += "&taskGroup=" + $scope.selectedTaskGroup;
                url += "&skip=" + $scope.pagination.skip + "&take=" + $scope.pagination.take;

                if ($scope.pagination.orderBy) {
                    url += "&orderBy=" + $scope.pagination.orderBy;
                    if ($scope.pagination.descending) url += "&descending=" + $scope.pagination.descending;
                }

                $http.get<Kitos.API.Models.IApiWrapper<any>>(url).then((result) => {
                    $scope.taskRefUsageList = result.data.response;

                    var paginationHeader = JSON.parse(result.headers("X-Pagination"));
                    $scope.totalCount = paginationHeader.TotalCount;
                    decorateTasks();
                }, (error) => {
                    notify.addErrorMessage("Kunne ikke hente opgaver!");
                });
            }

            function addUsage(refUsage, showMessage) {
                if (showMessage) var msg = notify.addInfoMessage("Opretter tilknytning...", false);

                var url = "api/taskUsage/";

                var payload = {
                    taskRefId: refUsage.taskRef.id,
                    orgUnitId: $scope.chosenOrgUnit.id
                };

                $http.post<Kitos.API.Models.IApiWrapper<any>>(url, payload).then((result) => {
                    refUsage.usage = result.data.response;
                    if (showMessage) msg.toSuccessMessage("Tilknytningen er oprettet");
                }, (error) => {
                    if (showMessage) msg.toErrorMessage("Fejl! Kunne ikke oprette tilknytningen!");
                });
            }

            function removeUsage(refUsage, showMessage) {
                if (showMessage) var msg = notify.addInfoMessage("Fjerner tilknytning...", false);

                var url = "api/taskUsage/" + refUsage.usage.id + "?organizationId=" + user.currentOrganizationId;

                $http.delete<Kitos.API.Models.IApiWrapper<any>>(url).then((result) => {
                    refUsage.usage = null;
                    if (showMessage) msg.toSuccessMessage("Tilknytningen er fjernet");
                }, (error) => {
                    if (showMessage) msg.toErrorMessage("Fejl! Kunne ikke fjerne tilknytningen!");
                });
            }

            function decorateTasks() {
                _.each($scope.taskRefUsageList, function (refUsage: { toggleUsage; usage; toggleStar; }) {
                    refUsage.toggleUsage = function () {
                        if (refUsage.usage) {
                            removeUsage(refUsage, true);
                        } else {
                            addUsage(refUsage, true);
                        }
                    };

                    refUsage.toggleStar = function () {
                        if (!refUsage.usage) return;

                        var payload = {
                            starred: !refUsage.usage.starred
                        };

                        var url = "api/taskUsage/" + refUsage.usage.id;
                        var msg = notify.addInfoMessage("Opdaterer...", false);
                        $http<Kitos.API.Models.IApiWrapper<any>>({ method: "PATCH", url: url + "?organizationId=" + user.currentOrganizationId, data: payload }).then(() => {
                            refUsage.usage.starred = !refUsage.usage.starred;
                            msg.toSuccessMessage("Feltet er opdateret");
                        }, (error) => {
                            msg.toErrorMessage("Fejl!");
                        });
                    };
                });
            }

            $scope.selectAllTasks = function () {
                _.each($scope.taskRefUsageList, function (refUsage: { usage }) {
                    if (!refUsage.usage) {
                        addUsage(refUsage, false);
                    }
                });
            };

            $scope.removeAllTasks = function () {
                _.each($scope.taskRefUsageList, function (refUsage: { usage }) {
                    if (refUsage.usage) {
                        removeUsage(refUsage, false);
                    }
                });
            };

            $scope.selectTaskGroup = function () {
                var url = "api/taskusage/taskGroup?orgUnitId=" + $scope.chosenOrgUnit.id + "&taskId=" + $scope.selectedTaskGroup;

                var msg = notify.addInfoMessage("Opretter tilknytning...", false);
                $http.post<Kitos.API.Models.IApiWrapper<any>>(url, null).then((result) => {
                    msg.toSuccessMessage("Tilknytningen er oprettet");
                    reload();
                }, (error) => {
                    msg.toErrorMessage("Fejl! Kunne ikke oprette tilknytningen!");
                });
            };

            $scope.removeTaskGroup = function () {
                var url = "api/taskusage?orgUnitId=" + $scope.chosenOrgUnit.id + "&taskId=" + $scope.selectedTaskGroup;

                var msg = notify.addInfoMessage("Fjerner tilknytning...", false);
                $http.delete<Kitos.API.Models.IApiWrapper<any>>(url).then(() => {
                    msg.toSuccessMessage("Tilknytningen er fjernet");
                    reload();
                }, (error) => {
                    msg.toErrorMessage("Fejl! Kunne ikke fjerne tilknytningen!");
                });
            };

            function reload() {
                $state.go(".", null, { reload: true });
            }

            $scope.dragEnabled = false;

            $scope.toggleDrag = function () {
                $scope.dragEnabled = !$scope.dragEnabled;
            };

            $scope.treeOptions = {
                accept: function (sourceNodeScope, destNodesScope, destIndex) {
                    return !angular.isUndefined(destNodesScope.$parentNodesScope);

                },
                dropped: function (e) {
                    var parent = e.dest.nodesScope.$nodeScope;

                    var sourceId = e.source.nodeScope.$modelValue.id;

                    var currentParentId = e.source.nodeScope.$parentNodeScope.$modelValue.id;

                    if (parent && currentParentId != parent.$modelValue.id) {
                        var parentId = parent.$modelValue.id;
                        var sourceId = e.source.nodeScope.$modelValue.id;

                        //SEND API PATCH CALL

                        var payload = {
                            parentId: parentId
                        };

                        var url = 'api/organizationunit/' + sourceId;
                        var msg = notify.addInfoMessage("Opdaterer...", false);
                        $http<Kitos.API.Models.IApiWrapper<any>>({ method: 'PATCH', url: url + '?organizationId=' + user.currentOrganizationId, data: payload }).then(() => {
                            msg.toSuccessMessage("Enheden er opdateret");
                            $scope.chooseOrgUnit(orgUnits);
                        }, (error) => {
                            msg.toErrorMessage("Fejl!");
                        });
                    }
                }
            };

            $scope.checkIfRoleIsAvailable = function (roleId) {
                var foundSelectedInOptions = _.find($scope.activeOrgRoles, function (option: any) { return option.Id === parseInt(roleId, 10) });
                return (foundSelectedInOptions);
            }

            $scope.getRoleName = function (roleId) {
                var role = _.find($scope.orgUnitRoles, function (role: any) { return role.Id === parseInt(roleId, 10) });
                return role.Name;
            }

            // activate
            loadUnits();
        }
    ]);
})(angular, app);
