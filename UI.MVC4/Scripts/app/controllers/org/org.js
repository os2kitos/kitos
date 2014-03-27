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

    app.controller('org.OrgViewCtrl', ['$rootScope', '$scope', '$http', 'growl', 'orgRolesHttp', function ($rootScope, $scope, $http, growl, orgRolesHttp) {
        $rootScope.page.title = 'Organisation';
        $rootScope.page.subnav = subnav;

        var userId = $rootScope.user.id;
        
        //cache
        var orgs = [];

        $scope.users = [];

        //flatten map of all loaded orgUnits
        $scope.orgUnits = [];

        $scope.orgRoles = {};
        _.each(orgRolesHttp.data.Response, function(orgRole) {
            $scope.orgRoles[orgRole.Id] = orgRole;
        });

        console.log($scope.orgRoles);
        
        
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
                
                //this url is waaay too long
                $http.get('api/organizationRight?hasWriteAccess&orgUnitId=' + node.Id + '&userId=' + userId)
                    .success(function (result) {

                        function flag(node) {
                            node.hasWriteAccessChecked = true;
                            node.hasWriteAccess = result.Response;

                            if (result.Response) _.each(node.Children, flag);
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
                    
                    //save user for later
                    $rootScope.users[right.User.Id] = right.User;
                });

            });
            
            $scope.chosenOrgUnit = node;
        };


        $scope.submitRight = function () {

            if (!$scope.selectedUser || !$scope.newRole) return;

            var unitId = $scope.chosenOrgUnit.Id;
            var role = $scope.orgRoles[parseInt($scope.newRole)];
            var user = $rootScope.users[$scope.selectedUser.id];

            var data = {
                'Object_Id': unitId,
                'Role_Id': role.Id,
                'User_Id': user.Id
            };

            $http.post("api/organizationright", data).success(function(result) {
                growl.addSuccessMessage(user.Name + " er knyttet i rollen " + role.Name);

                $scope.chosenOrgUnit.OrgRights.push({
                    'Object_Id': unitId,
                    'Role_Id': role.Id,
                    'User_Id': user.Id,
                    'User': user,
                    'userForSelect': { id: user.Id, text: user.Name },
                    'roleForSelect': role.Id,
                    show: true
                });
                
                $scope.newRole = "";
                $scope.selectedUser = "";
                
            }).error(function (result) {
                
                growl.addErrorMessage('Kunne ikke knytte ' + user.Name + ' i rollen!');
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
            var oId = right.Object_Id;
            var rId = right.Role_Id;
            var uId = right.User_Id;
            
            //new values
            var unitId = right.Object_Id;
            var role = $scope.orgRoles[right.roleForSelect];
            var user = $rootScope.users[right.userForSelect.id];

            //if nothing was changed, just exit edit-mode
            if (oId == unitId && rId == role.Id && uId == user.Id) {
                right.edit = false;
            }

            //otherwise, we should delete the old entry, then add a new one

            $http.delete("api/organizationright?oId=" + oId + "&rId=" + rId + "&uId=" + uId).success(function(deleteResult) {

                var data = {
                    'Object_Id': unitId,
                    'Role_Id': role.Id,
                    'User_Id': user.Id
                };

                $http.post("api/organizationright", data).success(function (result) {

                    right.Role_Id = role.Id;
                    right.User = user;
                    right.User_Id = user.Id;

                    right.edit = false;

                    growl.addSuccessMessage(user.Name + " er knyttet i rollen " + role.Name);

                }).error(function (result) {

                    //we successfully deleted the old entry, but didn't add a new one
                    //fuck

                    right.show = false;
                    
                    growl.addErrorMessage('Kunne ikke knytte ' + user.Name + ' i rollen!');
                });
                
            }).error(function (deleteResult) {
                
                //couldn't delete the old entry, just reset select options
                right.userForSelect = { id: right.User.id, text: right.User.Name };
                right.roleForSelect = right.Role_Id;

                growl.addErrorMessage('Kunne ikke knytte ' + user.Name + ' i rollen!');
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

    }]);

})(angular, app);