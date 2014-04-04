(function (ng, app) {

    var subnav = [
            { state: 'org-view', text: 'Organisation' }
    ];
    
    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('org-view', {
            url: '/organization',
            templateUrl: 'partials/org/org.html',
            controller: 'org.OrgViewCtrl',
            resolve: {
                orgRolesHttp: ['$http', function($http) {
                    return $http.get('api/organizationRole');
                }]
            }
        });

    }]);

    app.controller('org.OrgViewCtrl', ['$rootScope', '$scope', '$http', '$modal', 'growl', 'orgRolesHttp', '$q', function ($rootScope, $scope, $http, $modal, growl, orgRolesHttp, $q) {
        $rootScope.page.title = 'Organisation';
        $rootScope.page.subnav = subnav;

        var userId = $rootScope.user.id;

        //cache
        var orgs = [];

        //$scope.users = [];

        //flatten map of all loaded orgUnits
        $scope.orgUnits = {};

        $scope.orgRoles = {};
        _.each(orgRolesHttp.data.response, function(orgRole) {
            $scope.orgRoles[orgRole.id] = orgRole;
        });
        
        
        function flattenAndSave(orgUnit, inheritWriteAccess) {

            //restore previously saved settings
            if ($scope.orgUnits[orgUnit.id]) {

                var old = $scope.orgUnits[orgUnit.id];
                orgUnit.isOpen = old.isOpen;

        }
        
            $scope.orgUnits[orgUnit.id] = orgUnit;

            if (!inheritWriteAccess) {
                $http.get('api/organizationRight?hasWriteAccess&orgUnitId=' + orgUnit.id + '&userId=' + userId).success(function (result) {
                    orgUnit.hasWriteAccess = result.response;
                    
                    _.each(orgUnit.children, function(u) {
                        flattenAndSave(u, result.response);
        });

                });
        
            } else {
                
                orgUnit.hasWriteAccess = true;

                _.each(orgUnit.children, function (u) {
                    return flattenAndSave(u, true);
                });

            }
        }

        function loadUnits() {

            return $http.get('api/organizationunit?userId=' + userId).success(function(result) {
                $scope.nodes = result.response;

                _.each(result.response, function(u) {
                    flattenAndSave(u, false);
                });

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
            $http.get('api/organizationRight?organizationUnitId=' + node.id).success(function(data) {
                node.orgRights = data.response;

                _.each(node.orgRights, function (right) {
                    right.userForSelect = { id: right.user.id, text: right.user.name };
                    right.roleForSelect = right.roleId;
                    right.show = true;
                });

            });
            
            $scope.chosenOrgUnit = node;
        };


        $scope.submitRight = function () {

            if (!$scope.selectedUser || !$scope.newRole) return;

            var oId = $scope.chosenOrgUnit.id;
            var rId = parseInt($scope.newRole);
            var uId = $scope.selectedUser.id;

            var data = {
                "objectId": oId,
                "roleId": rId,
                "userId": uId
            };

            $http.post("api/organizationright", data).success(function (result) {
                growl.addSuccessMessage(result.response.user.name + " er knyttet i rollen");

                $scope.chosenOrgUnit.orgRights.push({
                    "objectId": result.response.objectId,
                    "roleId": result.response.roleId,
                    "userId": result.response.userId,
                    "user": result.response.user,
                    'userForSelect': { id: result.response.userId, text: result.response.user.name },
                    'roleForSelect': result.response.roleId,
                    show: true
                });
                
                $scope.newRole = "";
                $scope.selectedUser = "";
                
            }).error(function (result) {
                
                growl.addErrorMessage('Fejl!');
            });
        };

        $scope.deleteRight = function(right) {

            var oId = right.objectId;
            var rId = right.roleId;
            var uId = right.userId;

            $http.delete("api/organizationright?oId=" + oId + "&rId=" + rId + "&uId=" + uId).success(function(deleteResult) {
                right.show = false;
                growl.addSuccessMessage('Rollen er slettet!');
            }).error(function(deleteResult) {

                growl.addErrorMessage('Kunne ikke slette rollen!');
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

            $http.delete("api/organizationright?oId=" + oIdOld + "&rId=" + rIdOld + "&uId=" + uIdOld).success(function(deleteResult) {

                var data = {
                    "objectId": oIdNew,
                    "roleId": rIdNew,
                    "userId": uIdNew
                };

                $http.post("api/organizationright", data).success(function (result) {

                    right.roleId = result.response.roleId;
                    right.user = result.response.user;
                    right.userId = result.response.userId;

                    right.edit = false;

                    growl.addSuccessMessage(right.user.name + " er knyttet i rollen");

                }).error(function (result) {

                    //we successfully deleted the old entry, but didn't add a new one
                    //fuck

                    right.show = false;
                    
                    growl.addErrorMessage('Fejl!');
                });
                
            }).error(function (deleteResult) {
                
                //couldn't delete the old entry, just reset select options
                right.userForSelect = { id: right.user.id, text: right.user.name };
                right.roleForSelect = right.roleId;

                growl.addErrorMessage('Fejl!');
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

        $scope.editUnit = function (unit) {
            
            var modal = $modal.open({
                templateUrl: 'partials/org/edit-org-unit-modal.html',
                controller: ['$scope', '$modalInstance', function ($modalScope, $modalInstance) {

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
                        'newParent': unit.parentId,
                        'orgId': unit.organizationId,
                        'isRoot': unit.parentId == 0
                    };
                    
                    //only allow changing the parent if user is admin, and the unit isn't at the root
                    $modalScope.isAdmin = $rootScope.user.isGlobalAdmin || _.contains($rootScope.user.isLocalAdminFor, unit.organizationId);
                    $modalScope.canChangeParent = $modalScope.isAdmin && !$modalScope.orgUnit.isRoot;

                    $modalScope.patch = function () {
                        //don't allow duplicate submitting
                        if ($modalScope.submitting) return;
                        
                        var name = $modalScope.orgUnit.newName;
                        var parent = $modalScope.orgUnit.newParent;
                        
                        if (!name) return;

                        var data = {
                            "name": name
                        };

                        //only allow changing the parent if user is admin, and the unit isn't at the root
                        if ($modalScope.canChangeParent && parent) data["parentId"] = parent;

                        $modalScope.submitting = true;

                        var id = unit.id;

                        $http({ method: 'PATCH', url: "api/organizationUnit/" + id, data: data }).success(function(result) {
                            growl.addSuccessMessage(name + " er ændret.");

                            $modalInstance.close(result.response);
                        }).error(function(result) {
                            $modalScope.submitting = false;
                            growl.addErrorMessage("Fejl! " + name + " kunne ikke ændres!");
                        });

                    };

                    $modalScope.post = function () {
                        //don't allow duplicate submitting
                        if ($modalScope.submitting) return;

                        var name = $modalScope.newOrgUnit.name;
                        if (!name) return;

                        var parent = $modalScope.newOrgUnit.parent;
                        var orgId = $modalScope.newOrgUnit.orgId;

                        var data = {
                            "name": name,
                            "parentId": parent,
                            "organizationId": orgId
                        };

                        $modalScope.submitting = true;
                        
                        $http({ method: 'POST', url: "api/organizationUnit/", data: data }).success(function (result) {
                            growl.addSuccessMessage(name + " er gemt.");

                            $modalInstance.close(result.response);
                        }).error(function (result) {
                            $modalScope.submitting = false;
                            growl.addErrorMessage("Fejl! " + name + " kunne ikke gemmes!");
                        });
                    };

                    $modalScope.new = function () {
                        $modalScope.createNew = true;
                        $modalScope.newOrgUnit = {
                            name: 'Ny afdeling',
                            parent: $modalScope.orgUnit.id,
                            orgId: $modalScope.orgUnit.orgId
                        };

                        console.log($modalScope.newOrgUnit);
                    };

                    $modalScope.delete = function () {
                        //don't allow duplicate submitting
                        if ($modalScope.submitting) return;

                        $modalScope.submitting = true;

                        $http.delete("api/organizationUnit/" + unit.id).success(function() {
                            $modalInstance.close();
                            growl.addSuccessMessage(unit.name + " er slettet!");
                            
                        }).error(function() {
                            $modalScope.submitting = false;

                            growl.addErrorMessage("Fejl! " + unit.name + " kunne ikke slettes!");
                        });

                    };

                    $modalScope.cancel = function () {
                        $modalInstance.dismiss('cancel');
                    };
                }]
            });

            modal.result.then(function(returnedUnit) {

                loadUnits();

            });
        };

        $scope.updateTask = function (task) {
            task.selected = !task.selected;
            var orgUnitId = $scope.chosenOrgUnit.id;

            task.setChildrenState(task.selected);
            task.setParentState();

            if (task.selected === true) {
                $http.post('api/organizationUnit/' + orgUnitId + '?taskref=' + task.id);
            } else {
                $http.delete('api/organizationUnit/' + orgUnitId + '?taskref=' + task.id);
            }
        };

        function filterTasks() {
            var orgUnitParentId = $scope.chosenOrgUnit.parentId;

            if (orgUnitParentId === 0) {
                // root tree, show all
                _.each($scope.allTasksFlat, function(task) {
                    task.show = true;
                    task.canWrite = true;
                });
            } else {
                // node tree, show selected from parent
                $http.get('api/organizationUnit/' + orgUnitParentId + '?taskrefs').success(function (result) {
                    var selectedTasksOnParent = result.response;
                    _.each(selectedTasksOnParent, function (selectedTask) {
                        var foundTask = _.find($scope.allTasksFlat, function (task) {
                            return task.id === selectedTask.id;
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

            $http.get('api/organizationUnit/' + orgUnitId + '?taskrefs').success(function(result) {
                var selectedTasks = result.response;
                _.each(selectedTasks, function (selectTask) {
                    var foundTask = _.find($scope.allTasksFlat, function(task) {
                        return task.id === selectTask.id;
                    });
                    if (foundTask) {
                        foundTask.selected = true;
                        foundTask.setChildrenState(true);
                        foundTask.setParentState();

                        foundTask.handledByOrgUnit = mapIdToOrgUnit(selectTask.handledByOrgUnit);
                    }
                });
            });
        }
        
        function mapIdToOrgUnit(idList) {
            return _.map(idList, function (id) {
                var foundOrgUnit = _.find($scope.orgUnits, function (orgUnit) {
                    if (angular.isUndefined(orgUnit))
                        return false;
                    return orgUnit.id == id;
                });
                return foundOrgUnit.name;
            });
        };

        function toHierarchy(flatAry, idPropertyName, parentIdPropetyName, parentPropetyName, childPropertyName) {
            // default values
            parentPropetyName = typeof parentPropetyName !== 'undefined' ? parentPropetyName : 'parent';
            childPropertyName = typeof childPropertyName !== 'undefined' ?  childPropertyName : 'children';

            // sort by parent to get roots (roots are null) first, then we only need to iterrate once
            // example [1, 1, null, 2] -> [null, 1, 1, 2] (number is parent id)
            var sorted = _.sortBy(flatAry, function(obj) {
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

                    return _.every(this.children, function(child) {
                        if (isChecked === true) {
                            return child.selected === true;
                        } else {
                            return child.selected === false && child.indeterminate === false;
                        }
                    });
                };
                obj.setChildrenShown = function(isShown) {
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
                obj.setState = function(isChecked) {
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
                obj.setChildrenState = function(isChecked) {
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

                if (obj[parentIdPropetyName] === null) // is root
                    hierarchy.push(obj);
                else {
                    var parentObj = search(hierarchy, obj[parentIdPropetyName]);
                    if (!parentObj.hasOwnProperty(childPropertyName))
                        parentObj[childPropertyName] = [];

                    obj[parentPropetyName] = parentObj;
                    parentObj[childPropertyName].push(obj);
                }
            });

            return hierarchy;
        }

        $scope.$watch('chosenOrgUnit', function (newOrgUnit, oldOrgUnit) {
            if (newOrgUnit !== null) {
                getAllTasks().then(function () {
                    resetTasks();
                    filterTasks();
                    selectTasks();
                });
            }
        });

        function getAllTasks() {
            var deferred = $q.defer();
            // only get if not previously set
            if (!$scope.allTasksFlat) {
                $http.get('api/taskref').success(function (result) {
                    var tasks = result.response;
                    // flat array for easy searching
                    $scope.allTasksFlat = tasks;
                    // nested array for angular to generate tree in a repeat
                    $scope.allTasksTree = toHierarchy(tasks, 'id', 'parentId');
                    deferred.resolve();
                });
            } else {
                deferred.resolve();
            }
            return deferred.promise;
        }

        function resetTasks() {
            _.each($scope.allTasksFlat, function (task) {
                task.show = false;
                task.selected = false;
                task.indeterminate = false;
                task.canWrite = false;
                delete task.handledByOrgUnit;
            });
        }
    }]);

})(angular, app);