(function(ng, app) {
    app.config([
        '$stateProvider', function($stateProvider) {
            $stateProvider.state('organization.structure', {
                url: '/structure',
                templateUrl: 'partials/org/org.html',
                controller: 'org.StructureCtrl',
                resolve: {
                    orgRolesHttp: [
                        '$http', function($http) {
                            return $http.get('api/organizationRole');
                        }
                    ]
                }
            });
        }
    ]);

    app.controller('org.StructureCtrl', [
        '$scope', '$http', '$q', '$filter', '$modal', '$state', 'notify', 'orgRolesHttp', 'user',
        function($scope, $http, $q, $filter, $modal, $state, notify, orgRolesHttp, user) {
            $scope.orgId = user.currentOrganizationId;
            $scope.pagination = {
                skip: 0,
                take: 50
            };

            //cache
            var orgs = [];

            //flatten map of all loaded orgUnits
            $scope.orgUnits = {};

            $scope.orgRoles = {};
            _.each(orgRolesHttp.data.response, function(orgRole) {
                $scope.orgRoles[orgRole.id] = orgRole;
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
                    $http.get('api/organizationUnit/' + orgUnit.id + '?hasWriteAccess&organizationId=' + user.currentOrganizationId).success(function(result) {
                        orgUnit.hasWriteAccess = result.response;

                        _.each(orgUnit.children, function(u) {
                            flattenAndSave(u, result.response, orgUnit);
                        });

                    });
                } else {
                    orgUnit.hasWriteAccess = true;

                    _.each(orgUnit.children, function(u) {
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
                return $http.get('api/organizationunit?organization=' + user.currentOrganizationId).success(function(result) {
                    var rootNode = result.response;

                    $scope.nodes = [rootNode];

                    flattenAndSave(rootNode, false, null);

                    if ($scope.chosenOrgUnit) {

                        var chosenId = $scope.chosenOrgUnit.id;
                        var newChosen = $scope.orgUnits[chosenId];
                        $scope.chooseOrgUnit(newChosen);
                    }
                });
            }

            loadUnits();

            $scope.chosenOrgUnit = null;

            $scope.chooseOrgUnit = function(node) {
                if ($scope.chosenOrgUnit == node) return;

                //get organization related to the org unit
                if (!node.organization) {

                    //try get from cache
                    if (orgs[node.organizationId]) {

                        node.organization = orgs[node.organizationId];

                    } else {
                        //else get from server

                        $http.get('api/organization/' + node.organizationId).success(function(data) {
                            node.organization = data.response;

                            //save to cache
                            orgs[node.organizationId] = data.response;
                        });
                    }
                }

                //get org rights on the org unit and subtree
                $http.get('api/organizationUnitRights/' + node.id).success(function(data) {
                    node.orgRights = data.response;

                    _.each(node.orgRights, function(right) {
                        right.userForSelect = { id: right.user.id, text: right.user.fullName };
                        right.roleForSelect = right.roleId;
                        right.show = true;
                    });

                });

                $scope.chosenOrgUnit = node;

                loadTasks();
            };

            $scope.$watch("selectedUser", function() {
                $scope.submitRight();
            });

            /* the role of "medarbejder" */
            function getDefaultNewRole() {
                return 3;
            }

            $scope.newRole = getDefaultNewRole();

            $scope.submitRight = function() {
                if (!$scope.selectedUser || !$scope.newRole) return;

                var oId = $scope.chosenOrgUnit.id;
                var rId = parseInt($scope.newRole);
                var uId = $scope.selectedUser.id;

                if (!oId || !rId || !uId) return;

                var data = {
                    "roleId": rId,
                    "userId": uId
                };

                $http.post("api/organizationUnitRights/" + oId + '?organizationId=' + user.currentOrganizationId, data).success(function(result) {
                    notify.addSuccessMessage(result.response.user.fullName + " er knyttet i rollen");

                    $scope.chosenOrgUnit.orgRights.push({
                        "objectId": result.response.objectId,
                        "roleId": result.response.roleId,
                        "userId": result.response.userId,
                        "user": result.response.user,
                        'userForSelect': { id: result.response.userId, text: result.response.user.fullName },
                        'roleForSelect': result.response.roleId,
                        show: true
                    });

                    $scope.newRole = getDefaultNewRole();
                    $scope.selectedUser = "";
                }).error(function(result) {
                    notify.addErrorMessage('Fejl!');
                });
            };

            $scope.deleteRight = function(right) {
                var oId = right.objectId;
                var rId = right.roleId;
                var uId = right.userId;

                $http.delete("api/organizationUnitRights/" + oId + "?rId=" + rId + "&uId=" + uId + '&organizationId=' + user.currentOrganizationId).success(function (deleteResult) {
                    right.show = false;
                    notify.addSuccessMessage('Rollen er slettet!');
                }).error(function(deleteResult) {
                    notify.addErrorMessage('Kunne ikke slette rollen!');
                });
            };

            $scope.updateRight = function(right) {
                if (!right.roleForSelect || !right.userForSelect) return;

                //old values
                var oIdOld = right.objectId;
                var rIdOld = right.roleId;
                var uIdOld = right.userId;

                //new values
                var oIdNew = right.objectId;
                var rIdNew = right.roleForSelect;
                var uIdNew = right.userForSelect.id;

                //if nothing was changed, just exit edit-mode
                if (oIdOld == oIdNew && rIdOld == rIdNew && uIdOld == uIdNew) {
                    right.edit = false;
                }

                //otherwise, we should delete the old entry, then add a new one

                $http.delete("api/organizationUnitRights/" + oIdOld + "?rId=" + rIdOld + "&uId=" + uIdOld + '&organizationId=' + user.currentOrganizationId).success(function (deleteResult) {
                    var data = {
                        "roleId": rIdNew,
                        "userId": uIdNew
                    };

                    $http.post("api/organizationUnitRights/" + oIdNew + '?organizationId=' + user.currentOrganizationId, data).success(function (result) {
                        right.roleId = result.response.roleId;
                        right.user = result.response.user;
                        right.userId = result.response.userId;

                        right.edit = false;

                        notify.addSuccessMessage(right.user.fullName + " er knyttet i rollen");
                    }).error(function(result) {
                        // we successfully deleted the old entry, but didn't add a new one
                        // fuck

                        right.show = false;

                        notify.addErrorMessage('Fejl!');
                    });

                }).error(function(deleteResult) {

                    // couldn't delete the old entry, just reset select options
                    right.userForSelect = { id: right.user.id, text: right.user.fullName };
                    right.roleForSelect = right.roleId;

                    notify.addErrorMessage('Fejl!');
                });
            };

            $scope.rightSortBy = "orgUnitName";
            $scope.rightSortReverse = false;
            $scope.rightSort = function(right) {
                switch ($scope.rightSortBy) {
                case "orgUnitName":
                    return $scope.orgUnits[right.objectId].name;
                case "roleName":
                    return $scope.orgRoles[right.roleId].name;
                case "userName":
                    return right.user.name;
                case "userEmail":
                    return right.user.email;
                default:
                    return $scope.orgUnits[right.objectId].name;
                }
            };

            $scope.rightSortChange = function(val) {
                if ($scope.rightSortBy == val) {
                    $scope.rightSortReverse = !$scope.rightSortReverse;
                } else {
                    $scope.rightSortReverse = false;
                }

                $scope.rightSortBy = val;
            };

            $scope.editUnit = function(unit) {
                var modal = $modal.open({
                    templateUrl: 'partials/org/edit-org-unit-modal.html',
                    controller: [
                        '$scope', '$modalInstance', 'autofocus', function($modalScope, $modalInstance, autofocus) {
                            autofocus();

                            // edit or create-new mode
                            $modalScope.isNew = false;

                            // holds a list of org units, which the user can select as the parent
                            $modalScope.orgUnits = [];

                            // filter out those orgunits, that are outside the organisation
                            // or is currently a subdepartment of the unit
                            function filter(node) {
                                if (node.organizationId != unit.organizationId) return;

                                // this avoid every subdepartment
                                if (node.id == unit.id) return;

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
                                'newParent': unit.parentId,
                                'orgId': unit.organizationId,
                                'isRoot': unit.parentId == undefined
                            };

                            // only allow changing the parent if user is admin, and the unit isn't at the root
                            $modalScope.isAdmin = user.isGlobalAdmin || user.isLocalAdmin;
                            $modalScope.canChangeParent = $modalScope.isAdmin && !$modalScope.orgUnit.isRoot;

                            $modalScope.patch = function() {
                                // don't allow duplicate submitting
                                if ($modalScope.submitting) return;

                                var name = $modalScope.orgUnit.newName;
                                var parent = $modalScope.orgUnit.newParent;
                                var ean = $modalScope.orgUnit.newEan;

                                if (!name) return;

                                var data = {
                                    "name": name,
                                    "ean": ean
                                };

                                // only allow changing the parent if user is admin, and the unit isn't at the root
                                if ($modalScope.canChangeParent && parent) data["parentId"] = parent;

                                $modalScope.submitting = true;

                                var id = unit.id;

                                $http({ method: 'PATCH', url: "api/organizationUnit/" + id + '?organizationId=' + user.currentOrganizationId, data: data }).success(function (result) {
                                    notify.addSuccessMessage(name + " er ændret.");

                                    $modalInstance.close(result.response);
                                }).error(function(result) {
                                    $modalScope.submitting = false;
                                    notify.addErrorMessage("Fejl! " + name + " kunne ikke ændres!");
                                });

                            };

                            $modalScope.post = function() {
                                // don't allow duplicate submitting
                                if ($modalScope.submitting) return;

                                var name = $modalScope.newOrgUnit.name;
                                if (!name) return;

                                var parent = $modalScope.newOrgUnit.parent;
                                var orgId = $modalScope.newOrgUnit.orgId;
                                var ean = $modalScope.newOrgUnit.ean;

                                var data = {
                                    "name": name,
                                    "parentId": parent,
                                    "organizationId": orgId,
                                    "ean": ean
                                };

                                $modalScope.submitting = true;

                                $http({ method: 'POST', url: "api/organizationUnit/", data: data }).success(function(result) {
                                    notify.addSuccessMessage(name + " er gemt.");

                                    $modalInstance.close(result.response);
                                }).error(function(result) {
                                    $modalScope.submitting = false;
                                    notify.addErrorMessage("Fejl! " + name + " kunne ikke gemmes!");
                                });
                            };

                            $modalScope.new = function() {
                                autofocus();

                                $modalScope.createNew = true;
                                $modalScope.newOrgUnit = {
                                    name: '',
                                    parent: $modalScope.orgUnit.id,
                                    orgId: $modalScope.orgUnit.orgId
                                };
                            };

                            $modalScope.delete = function() {
                                //don't allow duplicate submitting
                                if ($modalScope.submitting) return;

                                $modalScope.submitting = true;

                                $http.delete("api/organizationUnit/" + unit.id + '&organizationId=' + user.currentOrganizationId).success(function () {
                                    $modalInstance.close();
                                    notify.addSuccessMessage(unit.name + " er slettet!");

                                }).error(function() {
                                    $modalScope.submitting = false;

                                    notify.addErrorMessage("Fejl! " + unit.name + " kunne ikke slettes!");
                                });

                            };

                            $modalScope.cancel = function() {
                                $modalInstance.dismiss('cancel');
                            };
                        }
                    ]
                });

                modal.result.then(function(returnedUnit) {
                    loadUnits();
                });
            };

            $scope.$watch("selectedTaskGroup", function(newVal, oldVal) {
                $scope.pagination.skip = 0;
                loadTasks();
            });

            $scope.$watchCollection('pagination', loadTasks);

            // default kle mode
            $scope.showAllTasks = true;
            // default kle sort order
            $scope.pagination.orderBy = 'taskKey';

            // change between show all tasks and only show active tasks
            $scope.changeTaskView = function() {
                $scope.showAllTasks = !$scope.showAllTasks;
                $scope.pagination.orderBy = $scope.showAllTasks ? 'taskKey' : 'taskRef.taskKey';
                $scope.pagination.skip = 0;
                loadTasks();
            };

            function loadTasks() {
                if (!$scope.chosenOrgUnit) return;

                var url = 'api/organizationUnit/' + $scope.chosenOrgUnit.id;

                if ($scope.showAllTasks) url += "?tasks";
                else url += '?usages';

                url += '&taskGroup=' + $scope.selectedTaskGroup;
                url += '&skip=' + $scope.pagination.skip + '&take=' + $scope.pagination.take;

                if ($scope.pagination.orderBy) {
                    url += '&orderBy=' + $scope.pagination.orderBy;
                    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                }

                $http.get(url).success(function(result, status, headers) {
                    $scope.taskRefUsageList = result.response;

                    var paginationHeader = JSON.parse(headers('X-Pagination'));
                    $scope.totalCount = paginationHeader.TotalCount;
                    decorateTasks();
                }).error(function() {
                    notify.addErrorMessage("Kunne ikke hente opgaver!");
                });
            }


            function addUsage(refUsage, showMessage) {
                if (showMessage) var msg = notify.addInfoMessage("Opretter tilknytning...", false);

                var url = 'api/taskUsage/';

                var payload = {
                    taskRefId: refUsage.taskRef.id,
                    orgUnitId: $scope.chosenOrgUnit.id
                };

                $http.post(url, payload).success(function(result) {
                    refUsage.usage = result.response;
                    if (showMessage) msg.toSuccessMessage("Tilknytningen er oprettet");
                }).error(function() {
                    if (showMessage) msg.toErrorMessage("Fejl! Kunne ikke oprette tilknytningen!");
                });
            }

            function removeUsage(refUsage, showMessage) {
                if (showMessage) var msg = notify.addInfoMessage("Fjerner tilknytning...", false);

                var url = 'api/taskUsage/' + refUsage.usage.id + '?organizationId=' + user.currentOrganizationId;

                $http.delete(url).success(function(result) {
                    refUsage.usage = null;
                    if (showMessage) msg.toSuccessMessage("Tilknytningen er fjernet");
                }).error(function() {
                    if (showMessage) msg.toErrorMessage("Fejl! Kunne ikke fjerne tilknytningen!");
                });
            }

            function decorateTasks() {

                _.each($scope.taskRefUsageList, function(refUsage) {
                    refUsage.toggleUsage = function() {
                        if (refUsage.usage) {
                            removeUsage(refUsage, true);
                        } else {
                            addUsage(refUsage, true);
                        }
                    };

                    refUsage.toggleStar = function() {
                        if (!refUsage.usage) return;

                        var payload = {
                            starred: !refUsage.usage.starred
                        };

                        var url = 'api/taskUsage/' + refUsage.usage.id;
                        var msg = notify.addInfoMessage("Opdaterer...", false);
                        $http({ method: 'PATCH', url: url + '?organizationId=' + user.currentOrganizationId, data: payload }).success(function () {
                            refUsage.usage.starred = !refUsage.usage.starred;
                            msg.toSuccessMessage("Feltet er opdateret");
                        }).error(function() {
                            msg.toErrorMessage("Fejl!");
                        });
                    };
                });
            }

            $scope.selectAllTasks = function() {
                _.each($scope.taskRefUsageList, function(refUsage) {
                    if (!refUsage.usage) {
                        addUsage(refUsage, false);
                    }
                });
            };

            $scope.removeAllTasks = function() {
                _.each($scope.taskRefUsageList, function(refUsage) {
                    if (refUsage.usage) {
                        removeUsage(refUsage, false);
                    }
                });
            };

            $scope.selectTaskGroup = function() {
                var url = 'api/taskusage?orgUnitId=' + $scope.chosenOrgUnit.id + '&taskId=' + $scope.selectedTaskGroup;
                
                var msg = notify.addInfoMessage("Opretter tilknytning...", false);
                $http.post(url).success(function() {
                    msg.toSuccessMessage("Tilknytningen er oprettet");
                    reload();
                }).error(function() {
                    msg.toErrorMessage("Fejl! Kunne ikke oprette tilknytningen!");
                });
            };

            $scope.removeTaskGroup = function () {
                var url = 'api/taskusage?orgUnitId=' + $scope.chosenOrgUnit.id + '&taskId=' + $scope.selectedTaskGroup;

                var msg = notify.addInfoMessage("Fjerner tilknytning...", false);
                $http.delete(url).success(function () {
                    msg.toSuccessMessage("Tilknytningen er fjernet");
                    reload();
                }).error(function () {
                    msg.toErrorMessage("Fejl! Kunne ikke fjerne tilknytningen!");
                });
            };

            function reload() {
                $state.go('.', null, { reload: true });
            }
        }
    ]);
})(angular, app);
