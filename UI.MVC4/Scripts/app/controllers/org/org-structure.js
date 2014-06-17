(function (ng, app) {

    function indent(level) {
        var result = "";
        for (var i = 0; i < level; i++) result += ".....";

        return result;
    };

    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

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

    }]);

    app.controller('org.StructureCtrl', ['$scope', '$http', '$q', '$filter', '$modal', 'notify', 'orgRolesHttp', 'user',
        function ($scope, $http, $q, $filter, $modal, notify, orgRolesHttp, user) {
            
            //cache
            var orgs = [];

            //flatten map of all loaded orgUnits
            $scope.orgUnits = {};

            $scope.orgRoles = {};
            _.each(orgRolesHttp.data.response, function (orgRole) {
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
                    $http.get('api/organizationUnit/' + orgUnit.id + '?hasWriteAccess').success(function (result) {
                        orgUnit.hasWriteAccess = result.response;

                        _.each(orgUnit.children, function (u) {
                            flattenAndSave(u, result.response, orgUnit);
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
                if (!user.defaultOrganizationUnitId) return;

                if (!unit || unit.id !== user.defaultOrganizationUnitId) return;

                open(unit);
                $scope.chooseOrgUnit(unit);

                function open(u) {
                    u.isOpen = true;
                    if (u.parent) open(u.parent);
                }
            }

            function loadUnits() {

                return $http.get('api/organizationunit?organization=' + user.currentOrganizationId).success(function (result) {
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

            $scope.chooseOrgUnit = function (node) {

                //get organization related to the org unit
                if (!node.organization) {

                    //try get from cache
                    if (orgs[node.organizationId]) {

                        node.organization = orgs[node.organizationId];

                    } else {
                        //else get from server

                        $http.get('api/organization/' + node.organizationId).success(function (data) {
                            node.organization = data.response;

                            //save to cache
                            orgs[node.organizationId] = data.response;
                        });
                    }
                }

                //get org rights on the org unit and subtree
                $http.get('api/organizationUnitRights/' + node.id).success(function (data) {
                    node.orgRights = data.response;

                    _.each(node.orgRights, function (right) {
                        right.userForSelect = { id: right.user.id, text: right.user.name };
                        right.roleForSelect = right.roleId;
                        right.show = true;
                    });

                });

                $scope.chosenOrgUnit = node;
            };

            $scope.$watch("selectedUser", function () {
                $scope.submitRight();
            });

            /* the role of "medarbejder", mmm, the taste of hardcode */
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

                $http.post("api/organizationUnitRights/" + oId, data).success(function (result) {
                    notify.addSuccessMessage(result.response.user.name + " er knyttet i rollen");

                    $scope.chosenOrgUnit.orgRights.push({
                        "objectId": result.response.objectId,
                        "roleId": result.response.roleId,
                        "userId": result.response.userId,
                        "user": result.response.user,
                        'userForSelect': { id: result.response.userId, text: result.response.user.name },
                        'roleForSelect': result.response.roleId,
                        show: true
                    });

                    $scope.newRole = getDefaultNewRole();
                    $scope.selectedUser = "";

                }).error(function (result) {

                    notify.addErrorMessage('Fejl!');
                });
            };

            $scope.deleteRight = function (right) {

                var oId = right.objectId;
                var rId = right.roleId;
                var uId = right.userId;

                $http.delete("api/organizationUnitRights/" + oId + "?rId=" + rId + "&uId=" + uId).success(function (deleteResult) {
                    right.show = false;
                    notify.addSuccessMessage('Rollen er slettet!');
                }).error(function (deleteResult) {

                    notify.addErrorMessage('Kunne ikke slette rollen!');
                });

            };

            $scope.updateRight = function (right) {

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

                $http.delete("api/organizationUnitRights/" + oIdOld + "?rId=" + rIdOld + "&uId=" + uIdOld).success(function (deleteResult) {

                    var data = {
                        "roleId": rIdNew,
                        "userId": uIdNew
                    };

                    $http.post("api/organizationUnitRights/" + oIdNew, data).success(function (result) {

                        right.roleId = result.response.roleId;
                        right.user = result.response.user;
                        right.userId = result.response.userId;

                        right.edit = false;

                        notify.addSuccessMessage(right.user.name + " er knyttet i rollen");

                    }).error(function (result) {

                        //we successfully deleted the old entry, but didn't add a new one
                        //fuck

                        right.show = false;

                        notify.addErrorMessage('Fejl!');
                    });

                }).error(function (deleteResult) {

                    //couldn't delete the old entry, just reset select options
                    right.userForSelect = { id: right.user.id, text: right.user.name };
                    right.roleForSelect = right.roleId;

                    notify.addErrorMessage('Fejl!');
                });
            };

            $scope.rightSortBy = "orgUnitName";
            $scope.rightSortReverse = false;
            $scope.rightSort = function (right) {
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

            $scope.rightSortChange = function (val) {
                if ($scope.rightSortBy == val) {
                    $scope.rightSortReverse = !$scope.rightSortReverse;
                } else {
                    $scope.rightSortReverse = false;
                }

                $scope.rightSortBy = val;
            };

            $scope.editUnit = function (unit) {

                var modal = $modal.open({
                    templateUrl: 'partials/org/edit-org-unit-modal.html',
                    controller: ['$scope', '$modalInstance', 'autofocus', function ($modalScope, $modalInstance, autofocus) {

                        autofocus();

                        //edit or create-new mode
                        $modalScope.isNew = false;

                        //holds a list of org units, which the user can select as the parent
                        $modalScope.orgUnits = [];

                        //filter out those orgunits, that are outside the organisation
                        //or is currently a subdepartment of the unit
                        function filter(node) {
                            if (node.organizationId != unit.organizationId) return;

                            //this avoid every subdepartment
                            if (node.id == unit.id) return;

                            $modalScope.orgUnits.push(node);

                            _.each(node.children, filter);
                        }
                        _.each($scope.nodes, filter);


                        //format the selected unit for editing
                        $modalScope.orgUnit = {
                            'id': unit.id,
                            'oldName': unit.name,
                            'newName': unit.name,
                            'newEan': unit.ean,
                            'newParent': unit.parentId,
                            'orgId': unit.organizationId,
                            'isRoot': unit.parentId == 0
                        };

                        //only allow changing the parent if user is admin, and the unit isn't at the root
                        $modalScope.isAdmin = user.isGlobalAdmin || _.contains(user.isLocalAdminFor, unit.organizationId);
                        $modalScope.canChangeParent = $modalScope.isAdmin && !$modalScope.orgUnit.isRoot;

                        $modalScope.patch = function () {
                            //don't allow duplicate submitting
                            if ($modalScope.submitting) return;

                            var name = $modalScope.orgUnit.newName;
                            var parent = $modalScope.orgUnit.newParent;
                            var ean = $modalScope.orgUnit.newEan;

                            if (!name) return;

                            var data = {
                                "name": name,
                                "ean": ean
                            };

                            //only allow changing the parent if user is admin, and the unit isn't at the root
                            if ($modalScope.canChangeParent && parent) data["parentId"] = parent;

                            $modalScope.submitting = true;

                            var id = unit.id;

                            $http({ method: 'PATCH', url: "api/organizationUnit/" + id, data: data }).success(function (result) {
                                notify.addSuccessMessage(name + " er ændret.");

                                $modalInstance.close(result.response);
                            }).error(function (result) {
                                $modalScope.submitting = false;
                                notify.addErrorMessage("Fejl! " + name + " kunne ikke ændres!");
                            });

                        };

                        $modalScope.post = function () {
                            //don't allow duplicate submitting
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

                            $http({ method: 'POST', url: "api/organizationUnit/", data: data }).success(function (result) {
                                notify.addSuccessMessage(name + " er gemt.");

                                $modalInstance.close(result.response);
                            }).error(function (result) {
                                $modalScope.submitting = false;
                                notify.addErrorMessage("Fejl! " + name + " kunne ikke gemmes!");
                            });
                        };

                        $modalScope.new = function () {
                            autofocus();

                            $modalScope.createNew = true;
                            $modalScope.newOrgUnit = {
                                name: '',
                                parent: $modalScope.orgUnit.id,
                                orgId: $modalScope.orgUnit.orgId
                            };
                        };

                        $modalScope.delete = function () {
                            //don't allow duplicate submitting
                            if ($modalScope.submitting) return;

                            $modalScope.submitting = true;

                            $http.delete("api/organizationUnit/" + unit.id).success(function () {
                                $modalInstance.close();
                                notify.addSuccessMessage(unit.name + " er slettet!");

                            }).error(function () {
                                $modalScope.submitting = false;

                                notify.addErrorMessage("Fejl! " + unit.name + " kunne ikke slettes!");
                            });

                        };

                        $modalScope.cancel = function () {
                            $modalInstance.dismiss('cancel');
                        };
                    }]
                });

                modal.result.then(function (returnedUnit) {

                    loadUnits();

                });
            };

            function notifySuccess() {
                notify.addSuccessMessage("Feltet er opdateret");
            }

            function notifyError() {
                notify.addErrorMessage("Fejl!");
            }

            $scope.updateTask = function (task) {
                task.selected = !task.selected;
                var orgUnitId = $scope.chosenOrgUnit.id;

                //task.setChildrenState(task.selected);
                //task.setParentState();

                if (task.selected === true) {
                    var data = {
                        taskRefId: task.id,
                        OrgUnitId: orgUnitId
                    };
                    $http.post('api/taskusage/', data).success(function (result) {
                        task.usageId = result.response.id;
                        notifySuccess();
                    }).error(notifyError);
                } else {
                    $http.delete('api/taskusage/' + task.usageId).success(function () {
                        task.starred = false;
                        delete task.delegatedTo;
                        notifySuccess();
                    }).error(notifyError);
                }
            };

            $scope.updateStar = function (task) {
                task.starred = !task.starred;
                $http({
                    method: 'PATCH',
                    url: 'api/taskusage/' + task.usageId,
                    data: { starred: task.starred }
                }).success(notifySuccess).error(notifyError);
            };

            function filterTasks() {
                var orgUnitParentId = $scope.chosenOrgUnit.parentId;

                if (orgUnitParentId === 0) {
                    // root tree, show all
                    _.each($scope.allTasksFlat, function (task) {
                        task.show = true;
                        task.canWrite = true;
                    });
                } else {
                    // node tree, show selected from parent
                    $http.get('api/taskusage/?orgUnitId=' + orgUnitParentId).success(function (result) {
                        var selectedTasksOnParent = result.response;
                        _.each(selectedTasksOnParent, function (selectedTask) {
                            var foundTask = _.find($scope.allTasksFlat, function (task) {
                                return task.id === selectedTask.usage.taskRefId;
                            });
                            if (foundTask) {
                                foundTask.show = true;
                                foundTask.canWrite = true;
                                foundTask.setChildrenShown(true);
                                foundTask.setParentShown(true);
                            }
                        });
                    });
                }
            }

            function selectTasks() {
                var orgUnitId = $scope.chosenOrgUnit.id;

                $http.get('api/taskusage/?orgUnitId=' + orgUnitId).success(function (result) {
                    var selectedTasks = result.response;
                    _.each(selectedTasks, function (selectTask) {
                        var foundTask = _.find($scope.allTasksFlat, function (task) {
                            return task.id === selectTask.usage.taskRefId;
                        });
                        if (foundTask) {
                            foundTask.usageId = selectTask.usage.id;
                            foundTask.selected = true;
                            foundTask.starred = selectTask.usage.starred;
                            //foundTask.setChildrenState(true);
                            //foundTask.setParentState();

                            foundTask.delegatedTo = mapIdToOrgUnit(_.map(selectTask.delegations, function (delegation) {
                                return delegation.usage.orgUnitId;
                            }));
                        }
                    });
                });
            }

            $scope.cleanKleFilter = function () {
                if ($scope.chosenOrgUnit.kleFilter.parent && $scope.chosenOrgUnit.kleFilter.parent.parentId === null) {
                    delete $scope.chosenOrgUnit.kleFilter.parent;
                }
                if ($scope.chosenOrgUnit.kleFilter.parentId === null) {
                    delete $scope.chosenOrgUnit.kleFilter.parentId;
                }
            };

            function mapIdToOrgUnit(idList) {
                if (idList.length === 0) {
                    return [];
                }

                return _.map(idList, function (id) {
                    var foundOrgUnit = _.find($scope.orgUnits, function (orgUnit) {
                        if (angular.isUndefined(orgUnit))
                            return false;
                        return orgUnit.id === id;
                    });
                    if (foundOrgUnit) {
                        return foundOrgUnit.name;
                    }
                    return 'Ukendt';
                });
            };

            function toHierarchy(flatAry, idPropertyName, parentIdPropetyName, parentPropetyName, childPropertyName) {
                // default values
                parentPropetyName = typeof parentPropetyName !== 'undefined' ? parentPropetyName : 'parent';
                childPropertyName = typeof childPropertyName !== 'undefined' ? childPropertyName : 'children';

                // sort by parent to get roots (roots are null) first, then we only need to iterrate once
                // example [1, 1, null, 2] -> [null, 1, 1, 2] (number is parent id)
                var sorted = _.sortBy(flatAry, function (obj) {
                    return obj[parentIdPropetyName];
                });

                function search(nestedAry, id) {
                    if (!nestedAry || !id)
                        throw new Error("Invalid argument(s)"); // abort if not valid input

                    for (var i = 0; i < nestedAry.length; i++) {
                        var obj = nestedAry[i];
                        if (obj[idPropertyName] === id) {
                            return obj;
                        } else if (obj.hasOwnProperty(childPropertyName)) { // has children, search them too
                            var found = search(obj[childPropertyName], id);
                            if (found) return found;
                        }
                    }
                }

                var hierarchy = [];
                _.each(sorted, function (obj) {
                    // define functions
                    obj.isAllChildren = function (isChecked) {
                        if (typeof isChecked !== 'boolean')
                            throw new Error('Argument must be a boolean');

                        return _.every(this.children, function (child) {
                            if (isChecked === true) {
                                return child.selected === true;
                            } else {
                                return child.selected === false && child.indeterminate === false;
                            }
                        });
                    };
                    obj.setChildrenShown = function (isShown) {
                        if (typeof isShown !== 'boolean')
                            throw new Error('Argument must be a boolean');

                        var children = this.children;
                        if (!children) return;

                        _.each(children, function (child) {
                            child.show = isShown;
                            child.canWrite = isShown;
                            child.setChildrenShown(isShown);
                        });
                    };
                    obj.setParentShown = function (isShown) {
                        var parent = this.parent;
                        if (!parent) return;
                        parent.show = isShown;
                        parent.setParentShown(isShown);
                    };
                    obj.setState = function (isChecked) {
                        if (isChecked === true) {
                            this.indeterminate = false;
                            this.selected = true;
                        } else if (isChecked === false) {
                            this.indeterminate = false;
                            this.selected = false;
                        } else {
                            this.indeterminate = true;
                            this.selected = false;
                        }
                    };
                    obj.setChildrenState = function (isChecked) {
                        if (typeof isChecked !== 'boolean')
                            throw new Error('Argument must be a boolean');

                        var children = this.children;
                        if (!children) return;

                        _.each(children, function (child) {
                            child.setState(isChecked);
                            child.setChildrenState(isChecked);
                        });
                    };
                    obj.setParentState = function () {
                        var parent = this.parent;
                        if (!parent)
                            return;
                        if (parent.isAllChildren(true)) {
                            parent.setState(true);
                        } else if (parent.isAllChildren(false)) {
                            parent.setState(false);
                        } else {
                            // if all children is neither true or false 
                            // then it must be a mix
                            // so we need to set the parent as not selected 
                            // and show the indeterminate state
                            parent.setState(null);
                        }
                        // cascade up the tree
                        parent.setParentState();
                    };

                    if (obj[parentIdPropetyName] === null) { // is root
                        obj.level = 0;
                        hierarchy.push(obj);
                    } else {
                        var parentObj = search(hierarchy, obj[parentIdPropetyName]);
                        if (!parentObj.hasOwnProperty(childPropertyName))
                            parentObj[childPropertyName] = [];

                        obj.level = parentObj.level + 1;
                        obj[parentPropetyName] = parentObj;
                        parentObj[childPropertyName].push(obj);
                    }
                });

                return hierarchy;
            }

            $scope.editTasks = function () {
                $scope.chosenOrgUnit.editTasks = !$scope.chosenOrgUnit.editTasks;

                if ($scope.chosenOrgUnit.editTasks) {
                    delete $scope.chosenOrgUnit.kleFilter.selected;
                } else {
                    $scope.chosenOrgUnit.kleFilter.selected = true;
                }
            };

            $scope.$watch('chosenOrgUnit', function (newOrgUnit, oldOrgUnit) {
                if (newOrgUnit) {
                    newOrgUnit.kleFilter = { type: 'KLE-Emne', selected: true };
                    var newRootOrgUnitId = getRootOrg(newOrgUnit).id;
                    var oldRootOrgUnitId;
                    if (oldOrgUnit === null) {
                        oldRootOrgUnitId = null;
                    } else {
                        oldRootOrgUnitId = getRootOrg(oldOrgUnit).id;
                    }

                    if (newRootOrgUnitId !== oldRootOrgUnitId) {
                        getAllTasks(newRootOrgUnitId).then(function () {
                            cleanup();
                        });
                    } else {
                        cleanup();
                    }
                }
            });

            function cleanup() {
                resetTasks();
                filterTasks();
                selectTasks();
            }

            function getRootOrg(orgUnit) {
                if (orgUnit.parent === null) {
                    return orgUnit;
                } else {
                    return getRootOrg(orgUnit.parent);
                }
            }

            function getAllTasks(rootOrgUnitId) {
                var deferred = $q.defer();
                $http.get('api/taskref?orgUnitId=' + rootOrgUnitId).success(function (result) {
                    var tasks = result.response;
                    // flat array for easy searching
                    $scope.allTasksFlat = tasks;
                    // nested array for angular to generate tree in a repeat
                    $scope.allTasksTree = toHierarchy(tasks, 'id', 'parentId');
                    deferred.resolve();
                });
                return deferred.promise;
            }

            function resetTasks() {
                _.each($scope.allTasksFlat, function (task) {
                    task.show = false;
                    task.selected = false;
                    task.indeterminate = false;
                    task.canWrite = false;
                    task.starred = false;
                    delete task.delegatedTo;
                    delete task.usageId;
                });
            }

            //function updateTree(rootOrgUnitId) {
            //    rootOrgUnitId = typeof rootOrgUnitId !== 'undefined' ? rootOrgUnitId : getRootOrg($scope.chosenOrgUnit).id;
            //    getAllTasks(rootOrgUnitId).then(function () {
            //        cleanup();
            //    });
            //}

            //$scope.modalAddTaskClick = function () {
            //    var modal = $modal.open({
            //        templateUrl: 'partials/org/add-task-modal.html',
            //        controller: ['$scope', '$modalInstance', function ($modalScope, $modalInstance) {
            //            $modalScope.orgName = $scope.chosenOrgUnit.organization.name;
            //            $modalScope.allTasks = $scope.allTasksFlat;
            //            $modalScope.task = {
            //                ownedByOrganizationUnitId: $scope.chosenOrgUnit.organization.id,
            //                uuid: '00000000-0000-0000-0000-000000000000',
            //                type: 'KLE',
            //                activeFrom: null,
            //                activeTo: null
            //            };

            //            $modalScope.ok = function () {
            //                var task = $modalScope.task;
            //                $http.post('api/taskref', task)
            //                    .success(function () {
            //                        notify.addSuccessMessage(task.taskKey + ' er oprettet');
            //                        updateTree();
            //                        $modalInstance.close();
            //                    })
            //                    .error(function () {
            //                        notify.addErrorMessage('Fejl');
            //                    });
            //            };

            //            $modalScope.cancel = function () {
            //                $modalInstance.dismiss('cancel');
            //            };
            //        }]
            //    });

            //    modal.result.then(
            //        //close
            //        function (result) {
            //            console.log(result);
            //        },
            //        //dismiss
            //        function (result) {
            //            var a = result;
            //        });
            //};

            $scope.indent = indent;

            var altRow = false;
            $scope.getAltRow = function () {
                altRow = !altRow;
                return altRow;
            };

            $scope.selectAll = function (toState) {
                var filter = $filter('filter');
                var filteredTasks = filter($scope.allTasksFlat, $scope.chosenOrgUnit.kleFilter);

                angular.forEach(filteredTasks, function (task) {
                    if (task.selected !== toState) {
                        $scope.updateTask(task);
                    }
                });
            };
        }]);
})(angular, app);