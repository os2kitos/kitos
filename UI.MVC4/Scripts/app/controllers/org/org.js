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

    app.controller('org.OrgViewCtrl', ['$rootScope', '$scope', '$http', '$modal', 'growl', 'orgRolesHttp', function ($rootScope, $scope, $http, $modal, growl, orgRolesHttp) {
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

            console.log("Flatten:");
            console.log(orgUnit);
            console.log("inheritWriteAccess: " + inheritWriteAccess);

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
                if (orgs[node.organization_Id]) {
                    
                    node.organization = orgs[node.organization_Id];
                    
                } else {
                    //else get from server
                    
                    $http.get('api/organization/' + node.organization_Id).success(function (data) {
                        node.organization = data.response;
                        
                        //save to cache
                        orgs[node.organization_Id] = data.response;
                    });
                }
            }
            
            //get org rights on the org unit and subtree
            $http.get('api/organizationRight?organizationUnitId=' + node.id).success(function(data) {
                node.orgRights = data.response;

                _.each(node.orgRights, function (right) {
                    right.userForSelect = { id: right.user.id, text: right.user.name };
                    right.roleForSelect = right.role_Id;
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
                "object_Id": oId,
                "role_Id": rId,
                "user_Id": uId
            };

            $http.post("api/organizationright", data).success(function (result) {
                growl.addSuccessMessage(result.response.user.name + " er knyttet i rollen");

                $scope.chosenOrgUnit.orgRights.push({
                    "object_Id": result.response.object_Id,
                    "role_Id": result.response.role_Id,
                    "user_Id": result.response.user_Id,
                    "user": result.response.user,
                    'userForSelect': { id: result.response.user_Id, text: result.response.user.name },
                    'roleForSelect': result.response.role_Id,
                    show: true
                });
                
                $scope.newRole = "";
                $scope.selectedUser = "";
                
            }).error(function (result) {
                
                growl.addErrorMessage('Fejl!');
            });
        };

        $scope.deleteRight = function(right) {

            var oId = right.object_Id;
            var rId = right.role_Id;
            var uId = right.user_Id;

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
            var oIdOld = right.object_Id;
            var rIdOld = right.role_Id;
            var uIdOld = right.user_Id;
            
            //new values
            var oIdNew = right.object_Id;
            var rIdNew = right.roleForSelect;
            var uIdNew = right.userForSelect.id;
            
            //if nothing was changed, just exit edit-mode
            if (oIdOld == oIdNew && rIdOld == rIdNew && uIdOld == uIdNew) {
                right.edit = false;
            }

            //otherwise, we should delete the old entry, then add a new one

            $http.delete("api/organizationright?oId=" + oIdOld + "&rId=" + rIdOld + "&uId=" + uIdOld).success(function(deleteResult) {

                var data = {
                    "object_Id": oIdNew,
                    "role_Id": rIdNew,
                    "user_Id": uIdNew
                };

                $http.post("api/organizationright", data).success(function (result) {

                    right.role_Id = result.response.role_Id;
                    right.user = result.response.user;
                    right.user_Id = result.response.user_Id;

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
                right.roleForSelect = right.role_Id;

                growl.addErrorMessage('Fejl!');
            });
        };

        $scope.rightSortBy = "orgUnitName";
        $scope.rightSortReverse = false;
        $scope.rightSort = function(right) {
            switch ($scope.rightSortBy) {
                case "orgUnitName":
                    return $scope.orgUnits[right.object_Id].name;
                case "roleName":
                    return $scope.orgRoles[right.role_Id].name;
                case "userName":
                    return right.user.name;
                case "userEmail":
                    return right.user.email;
                default:
                    return $scope.orgUnits[right.object_Id].name;
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
                        if (node.organization_Id != unit.organization_Id) return;
                        
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
                        'newParent': unit.parent_Id,
                        'orgId': unit.organization_Id,
                        'isRoot': unit.parent_Id == 0
                    };
                    
                    //only allow changing the parent if user is admin, and the unit isn't at the root
                    $modalScope.isAdmin = $rootScope.user.isGlobalAdmin || _.contains($rootScope.user.isLocalAdminFor, unit.organization_Id);
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
                        if ($modalScope.canChangeParent && parent) data["parent_Id"] = parent;

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
                            "parent_Id": parent,
                            "organization_Id": orgId
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

    }]);

})(angular, app);