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


        function flattenAndLoad(orgUnit, inheritWriteAccess) {
            $scope.orgUnits[orgUnit.Id] = orgUnit;

            if (!inheritWriteAccess) {
                $http.get('api/organizationRight?hasWriteAccess&orgUnitId=' + orgUnit.Id + '&userId=' + userId).success(function (result) {
                    orgUnit.hasWriteAccess = result.Response;
                    
                    _.each(orgUnit.Children, function(u) {
                        return flattenAndLoad(u, result.Response);
                    });

                });
                
            } else {
                
                orgUnit.hasWriteAccess = true;

                _.each(orgUnit.Children, function (u) {
                    return flattenAndLoad(u, true);
                });

            }
        }

        $http.get('api/organizationunit?userId=' + userId).success(function(result) {
            $scope.nodes = result.Response;

            _.each(result.Response, flattenAndLoad);
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

        $scope.addUnit = function(parent) {
            $scope.editUnit({ 'Parent_Id': parent.Id }, true);
        };
        
        $scope.editUnit = function (unit, createNew) {
            var oldParentId = unit.Parent_Id;

            var modal = $modal.open({
                templateUrl: 'partials/org/edit-org-unit-modal.html',
                controller: ['$scope', '$modalInstance', function ($modalScope, $modalInstance) {

                    $modalScope.orgUnits = $scope.orgUnits;

                    console.log($modalScope.orgUnits);
                    
                    $modalScope.orgUnit = unit;

                    $modalScope.save = function () {

                        if ($modalScope.orgUnit.form.$invalid) return;

                        var name = $modalScope.orgUnit.Name;
                        var parent = $modalScope.orgUnit.Parent_Id;
                        
                        //try to get the organization id from 
                        var orgId = $modalScope.orgUnit.Organization_Id;
                        if (!orgId) {
                            //take parent's organization id
                            orgId = $modalScope.orgUnits[parent].Organization_Id;
                        }

                        var data = {
                            'Name': name,
                            'Parent_Id': parent,
                            'Organization_Id': orgId
                        };

                        $modalScope.submitting = true;
                        
                        if (createNew) {
                            $http.post("api/organizationUnit", data).success(function(result) {
                                growl.addSuccessMessage(name + " er oprettet i KITOS");

                                $modalInstance.close(result.Response);
                            }).error(function(result) {
                                $modalScope.submitting = false;
                                growl.addErrorMessage("Fejl! " + name + " blev ikke oprettet i KITOS!");
                            });
                        } else {
                            var id = $modalScope.orgUnit.Id;
                            
                            $http({ method: 'PATCH', url: "api/organizationUnit/" + id, data: data }).success(function (result) {
                                growl.addSuccessMessage(name + " er ændret.");

                                $modalInstance.close(result.Response);
                            }).error(function (result) {
                                $modalScope.submitting = false;
                                growl.addErrorMessage("Fejl! " + name + " kunne ikke ændres!");
                            });
                        }
                    };

                    $modalScope.cancel = function () {
                        $modalInstance.dismiss('cancel');
                    };
                }]
            });

            modal.result.then(function (returnedUnit) {

                if (!createNew) {
                    
                    //remove old reference
                    var oldParent = $scope.orgUnits[oldParentId];
                    oldParent.Children = _.reject(oldParent.Children, function(u) {
                        return u.Id == returnedUnit.Id;
                    });
                }
                
                //save to cache
                $scope.orgUnits[returnedUnit.Id] = returnedUnit;

                //update new parent element
                var newParent = $scope.orgUnits[returnedUnit.Parent_Id];
                newParent.Children.push(returnedUnit);

                console.log(newParent);
                
            });
        };

    }]);

})(angular, app);