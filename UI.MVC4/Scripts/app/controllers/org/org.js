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
        _.each(orgRolesHttp.data.Response, function(orgRole) {
            $scope.orgRoles[orgRole.Id] = orgRole;
        });


        function flattenAndSave(orgUnit, inheritWriteAccess) {
            if ($scope.orgUnits[orgUnit.Id]) {

                var old = $scope.orgUnits[orgUnit.Id];
                orgUnit.isOpen = old.isOpen;

            }
            
            $scope.orgUnits[orgUnit.Id] = orgUnit;

            if (!inheritWriteAccess) {
                $http.get('api/organizationRight?hasWriteAccess&orgUnitId=' + orgUnit.Id + '&userId=' + userId).success(function (result) {
                    orgUnit.hasWriteAccess = result.Response;
                    
                    _.each(orgUnit.Children, function(u) {
                        return flattenAndSave(u, result.Response);
                    });

                });
                
            } else {
                
                orgUnit.hasWriteAccess = true;

                _.each(orgUnit.Children, function (u) {
                    return flattenAndSave(u, true);
                });

            }
        }

        function loadUnits() {

            return $http.get('api/organizationunit?userId=' + userId).success(function(result) {
                $scope.nodes = result.Response;

                _.each(result.Response, flattenAndSave);

                if ($scope.chosenOrgUnit) {
                    
                    var chosenId = $scope.chosenOrgUnit.Id;
                    var newChosen = $scope.orgUnits[chosenId];
                    $scope.chooseOrgUnit(newChosen);
                }
            });
        }

        loadUnits();

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
                        if (node.Organization_Id != unit.Organization_Id) return;
                        
                        //this avoid every subdepartment
                        if (node.Id == unit.Id) return;

                        $modalScope.orgUnits.push(node);
                        
                        _.each(node.Children, filter);
                    }
                    _.each($scope.nodes, filter);


                    //format the selected unit for editing
                    $modalScope.orgUnit = {
                        'id': unit.Id,
                        'oldName': unit.Name,
                        'newName': unit.Name,
                        'newParent': unit.Parent_Id,
                        'orgId': unit.Organization_Id,
                        'isRoot': unit.Parent_Id == 0
                    };
                    
                    //only allow changing the parent if user is admin, and the unit isn't at the root
                    $modalScope.isAdmin = $rootScope.user.isGlobalAdmin || _.contains($rootScope.user.isLocalAdminFor, unit.Organization_Id);
                    $modalScope.canChangeParent = $modalScope.isAdmin && !$modalScope.orgUnit.isRoot;

                    $modalScope.patch = function () {
                        
                        var name = $modalScope.orgUnit.newName;
                        var parent = $modalScope.orgUnit.newParent;
                        
                        if (!name) return;

                        var data = {
                            'Name': name
                        };

                        if ($modalScope.canChangeParent && parent) data['Parent_Id'] = parent;

                        $modalScope.submitting = true;
                        
                        var id = $modalScope.orgUnit.id;

                        $http({ method: 'PATCH', url: "api/organizationUnit/" + id, data: data }).success(function(result) {
                            growl.addSuccessMessage(name + " er ændret.");

                            $modalInstance.close(result.Response);
                        }).error(function(result) {
                            $modalScope.submitting = false;
                            growl.addErrorMessage("Fejl! " + name + " kunne ikke ændres!");
                        });

                    };

                    $modalScope.post = function() {
                        
                        var name = $modalScope.newOrgUnit.name;
                        if (!name) return;

                        var parent = $modalScope.newOrgUnit.parent;
                        var orgId = $modalScope.newOrgUnit.orgId;

                        var data = {
                            'Name': name,
                            'Parent_Id': parent,
                            'Organization_Id': orgId
                        };

                        $modalScope.submitting = true;
                        
                        $http({ method: 'POST', url: "api/organizationUnit/", data: data }).success(function (result) {
                            growl.addSuccessMessage(name + " er gemt.");

                            $modalInstance.close(result.Response);
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