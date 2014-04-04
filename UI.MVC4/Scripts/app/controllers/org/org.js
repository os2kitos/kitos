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

    app.controller('org.OrgViewCtrl', ['$rootScope', '$scope', '$http', 'growl', 'orgRolesHttp', '$q', function ($rootScope, $scope, $http, growl, orgRolesHttp, $q) {
        $rootScope.page.title = 'Organisation';
        $rootScope.page.subnav = subnav;

        var userId = $rootScope.user.id;

        //cache
        var orgs = [];

        //$scope.users = [];

        //flatten map of all loaded orgUnits
        $scope.orgUnits = [];

        $scope.orgRoles = {};
        _.each(orgRolesHttp.data.Response, function(orgRole) {
            $scope.orgRoles[orgRole.Id] = orgRole;
        });
        
        
        function flatten(orgUnit) {
            $scope.orgUnits[orgUnit.Id] = orgUnit;

            _.each(orgUnit.Children, flatten);
        }
        
        $http.get('api/organizationunit?userId=' + userId).success(function(result) {
            $scope.nodes = result.Response;

            _.each(result.Response, flatten);
        });

        
        $scope.chosenOrgUnit = null;
        
        $scope.chooseOrgUnit = function (node) {
            
            //get organization related to the org unit
            if (!node.Organization) {

                //try get from cache
                if (orgs[node.Organization_Id]) {
                    
                    node.Organization = orgs[node.Organization_Id];
                    
                } else {
                    //else get from server
                    
                    $http.get('api/organization/' + node.Organization_Id).success(function (data) {
                        node.Organization = data.Response;
                        
                        //save to cache
                        orgs[node.Organization_Id] = data.Response;
                    });
                }
            }

            if (!node.writeAccessChecked) {
                
                //this url is pretty too long
                $http.get('api/organizationRight?hasWriteAccess&orgUnitId=' + node.Id + '&userId=' + userId)
                    .success(function (result) {

                        function flag(myNode) {
                            myNode.hasWriteAccessChecked = true;
                            myNode.hasWriteAccess = result.Response;

                            if (result.Response) _.each(myNode.Children, flag);
                        }

                        flag(node);
                    }
                );
            }
            
            //get org rights on the org unit and subtree
            $http.get('api/organizationRight?organizationUnitId=' + node.Id).success(function(data) {
                node.OrgRights = data.Response;

                _.each(node.OrgRights, function (right) {
                    right.userForSelect = { id: right.User.Id, text: right.User.Name };
                    right.roleForSelect = right.Role_Id;
                    right.show = true;
                });

            });
            
            $scope.chosenOrgUnit = node;
        };


        $scope.submitRight = function () {

            if (!$scope.selectedUser || !$scope.newRole) return;

            var oId = $scope.chosenOrgUnit.Id;
            var rId = parseInt($scope.newRole);
            var uId = $scope.selectedUser.id;

            var data = {
                'Object_Id': oId,
                'Role_Id': rId,
                'User_Id': uId
            };

            $http.post("api/organizationright", data).success(function (result) {
                growl.addSuccessMessage(result.Response.User.Name + " er knyttet i rollen");

                $scope.chosenOrgUnit.OrgRights.push({
                    'Object_Id': result.Response.Object_Id,
                    'Role_Id': result.Response.Role_Id,
                    'User_Id': result.Response.User_Id,
                    'User': result.Response.User,
                    'userForSelect': { id: result.Response.User_Id, text: result.Response.User.Name },
                    'roleForSelect': result.Response.Role_Id,
                    show: true
                });
                
                $scope.newRole = "";
                $scope.selectedUser = "";
                
            }).error(function (result) {
                
                growl.addErrorMessage('Fejl!');
            });
        };

        $scope.deleteRight = function(right) {

            var oId = right.Object_Id;
            var rId = right.Role_Id;
            var uId = right.User_Id;

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
            var oIdOld = right.Object_Id;
            var rIdOld = right.Role_Id;
            var uIdOld = right.User_Id;
            
            //new values
            var oIdNew = right.Object_Id;
            var rIdNew = right.roleForSelect;
            var uIdNew = right.userForSelect.id;
            
            //if nothing was changed, just exit edit-mode
            if (oIdOld == oIdNew && rIdOld == rIdNew && uIdOld == uIdNew) {
                right.edit = false;
            }

            //otherwise, we should delete the old entry, then add a new one

            $http.delete("api/organizationright?oId=" + oIdOld + "&rId=" + rIdOld + "&uId=" + uIdOld).success(function(deleteResult) {

                var data = {
                    'Object_Id': oIdNew,
                    'Role_Id': rIdNew,
                    'User_Id': uIdNew
                };

                $http.post("api/organizationright", data).success(function (result) {

                    right.Role_Id = result.Response.Role_Id;
                    right.User = result.Response.User;
                    right.User_Id = result.Response.User_Id;

                    right.edit = false;

                    growl.addSuccessMessage(right.User.Name + " er knyttet i rollen");

                }).error(function (result) {

                    //we successfully deleted the old entry, but didn't add a new one
                    //fuck

                    right.show = false;
                    
                    growl.addErrorMessage('Fejl!');
                });
                
            }).error(function (deleteResult) {
                
                //couldn't delete the old entry, just reset select options
                right.userForSelect = { id: right.User.id, text: right.User.Name };
                right.roleForSelect = right.Role_Id;

                growl.addErrorMessage('Fejl!');
            });
        };

        $scope.rightSortBy = "orgUnitName";
        $scope.rightSortReverse = false;
        $scope.rightSort = function(right) {
            switch ($scope.rightSortBy) {
                case "orgUnitName":
                    return $scope.orgUnits[right.Object_Id].Name;
                case "roleName":
                    return $scope.orgRoles[right.Role_Id].Name;
                case "userName":
                    return right.User.Name;
                case "userEmail":
                    return right.User.Email;
                default:
                    return $scope.orgUnits[right.Object_Id].Name;
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

        $scope.updateTask = function (task) {
            task.selected = !task.selected;
            var orgUnitId = $scope.chosenOrgUnit.Id;

            task.setChildrenState(task.selected);
            // if all children has been selected then send parent task
            task.setParentState();

            if (task.selected === true) {
                //$http.post('api/organizationUnit/' + orgUnitId + '?taskref=' + task.Id).success(updateLists);
            } else {
                //$http.delete('api/organizationUnit/' + orgUnitId + '?taskref=' + task.Id).success(updateLists);
            }
        };

        function filterTasks() {
            var orgUnitParentId = $scope.chosenOrgUnit.Parent_Id;

            if (orgUnitParentId === 0) {
                // root tree, show all
                _.each($scope.allTasksFlat, function(task) {
                    task.show = true;
                });
            } else {
                // node tree, show selected from parent
                $http.get('api/organizationUnit/' + orgUnitParentId + '?taskrefs').success(function (result) {
                    var selectedTasksOnParent = result.Response;
                    _.each(selectedTasksOnParent, function (selectedTask) {
                        var foundTask = _.find($scope.allTasksFlat, function (task) {
                            return task.Id === selectedTask.Id;
                        });
                        if (foundTask) {
                            foundTask.show = true;
                            foundTask.setChildrenShown(true);
                            foundTask.setParentShown(true);
                        }
                    });
                });
            }
        }

        function selectTasks() {
            var orgUnitId = $scope.chosenOrgUnit.Id;

            $http.get('api/organizationUnit/' + orgUnitId + '?taskrefs').success(function(result) {
                var selectedTasks = result.Response;
                _.each(selectedTasks, function (selectTask) {
                    var foundTask = _.find($scope.allTasksFlat, function(task) {
                        return task.Id === selectTask.Id;
                    });
                    if (foundTask) {
                        foundTask.selected = true;
                        foundTask.setChildrenState(true);
                        foundTask.setParentState();
                    }
                });
            });
        }

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
                        child.setChildrenShown(isShown);
                    });
                };
                obj.setParentShown = function (isShown) {
                    var parent = this.parent;
                    if (!parent) return;
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
                    
                    _.each(children, function(child) {
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

        $scope.isTasksEditable = false;
        $scope.mapIdToOrgUnit = function (idList) {
            return _.map(idList, function(id) {
                var foundOrgUnit = _.find($scope.orgUnits, function (orgUnit) {
                    if (angular.isUndefined(orgUnit))
                        return false;
                    
                    return orgUnit.Id == id;
                });
                return foundOrgUnit.Name;
            });
        };

        $scope.$watch('chosenOrgUnit', function (newOrgUnit, oldOrgUnit) {
            if (newOrgUnit !== null) {
                getAllTasks().then(function() {
                    resetTasks();
                    filterTasks();
                    selectTasks();
                });
                //updateLists(true);
            }
                
        });

        function getAllTasks() {
            var deferred = $q.defer();
            // only get if not previously set
            if (!$scope.allTasksFlat) {
                $http.get('api/taskref').success(function (result) {
                    var tasks = result.Response;
                    // flat array for easy searching
                    $scope.allTasksFlat = tasks;
                    // nested array for angular to generate tree in a repeat
                    $scope.allTasksTree = toHierarchy(tasks, 'Id', 'Parent_Id');
                    deferred.resolve();
                });
            } else {
                deferred.resolve();
            }
            return deferred.promise;
        }

        function resetTasks() {
            _.each($scope.allTasksFlat, function(task) {
                task.show = false;
                task.selected = false;
            });
        }
    }]);

})(angular, app);