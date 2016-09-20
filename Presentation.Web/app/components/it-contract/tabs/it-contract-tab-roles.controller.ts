(function(ng, app) {
    app.config(['$stateProvider', function($stateProvider) {
        $stateProvider.state('it-contract.edit.roles', {
            url: '/roles',
            templateUrl: 'app/components/it-contract/tabs/it-contract-tab-roles.view.html',
            controller: 'contract.EditRolesCtrl',
            resolve: {
                itContractRights: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itcontractright/" + $stateParams.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                itContractRoles: ['$http', function ($http) {
                    return $http.get("api/itcontractrole/?nonsuggestions=")
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                user: ['userService', function(userService) {
                    return userService.getUser().then(function(user) {
                        return user;
                    });
                }]
            }
        });
    }]);

    app.controller('contract.EditRolesCtrl', ['$scope', '$http', 'notify', 'contract', 'itContractRights', 'itContractRoles', 'user',
        function ($scope, $http, notify, contract, itContractRights, itContractRoles, user) {
            var contractId = contract.id;
            $scope.orgId = user.currentOrganizationId;

            //normal user roles
            $scope.activeItContractRoles = _.filter(itContractRoles, { isActive: true });
            $scope.itContractRoles = itContractRoles;
            $scope.newRole = itContractRoles.length > 0 ? 1 : 0;

            $scope.rights = [];
            _.each(itContractRights, function (right: { role; roleId; show; userForSelect; roleForSelect; user; }) {
                right.role = _.find(itContractRoles, { id: right.roleId });
                right.show = true;

                right.userForSelect = { id: right.user.id, text: right.user.fullName };
                right.roleForSelect = right.roleId;

                $scope.rights.push(right);
            });

            $scope.$watch("selectedUser", function () {
                $scope.submitRight();
            });

            $scope.submitRight = function () {
                if (!$scope.selectedUser || !$scope.newRole) return;

                var rId = parseInt($scope.newRole);
                var uId = $scope.selectedUser.id;

                if (!rId || !uId) return;

                var data = {
                    "roleId": rId,
                    "userId": uId
                };

                $http.post("api/itcontractright/" + contractId + '?organizationId=' + user.currentOrganizationId, data).success(function (result) {
                    notify.addSuccessMessage(result.response.user.fullName + " er knyttet i rollen");

                    $scope.rights.push({
                        objectId: result.response.objectId,
                        roleId: result.response.roleId,
                        userId: result.response.userId,
                        user: result.response.user,
                        userForSelect: { id: result.response.userId, text: result.response.user.fullName },
                        roleForSelect: result.response.roleId,
                        role: _.find(itContractRoles, { id: result.response.roleId }),
                        show: true
                    });

                    $scope.newRole = 1;
                    $scope.selectedUser = "";

                }).error(function (result) {

                    notify.addErrorMessage('Fejl!');
                });
            };

            $scope.deleteRight = function (right) {

                var rId = right.roleId;
                var uId = right.userId;

                $http.delete("api/itcontractright/" + contractId + "?rId=" + rId + "&uId=" + uId + '&organizationId=' + user.currentOrganizationId).success(function (deleteResult) {
                    right.show = false;
                    notify.addSuccessMessage('Rollen er slettet!');
                }).error(function (deleteResult) {

                    notify.addErrorMessage('Kunne ikke slette rollen!');
                });

            };

            $scope.updateRight = function (right) {

                if (!right.roleForSelect || !right.userForSelect) return;

                //old values
                var rIdOld = right.roleId;
                var uIdOld = right.userId;

                //new values
                var rIdNew = right.roleForSelect;
                var uIdNew = right.userForSelect.id;

                //if nothing was changed, just exit edit-mode
                if (rIdOld == rIdNew && uIdOld == uIdNew) {
                    right.edit = false;
                }

                //otherwise, we should delete the old entry, then add a new one

                $http.delete("api/itcontractright/" + contractId + "?rId=" + rIdOld + "&uId=" + uIdOld + '&organizationId=' + user.currentOrganizationId).success(function (deleteResult) {

                    var data = {
                        "roleId": rIdNew,
                        "userId": uIdNew
                    };

                    $http.post("api/itcontractright/" + contractId + '?organizationId=' + user.currentOrganizationId, data).success(function (result) {

                        right.roleId = result.response.roleId;
                        right.user = result.response.user;
                        right.userId = result.response.userId;

                        right.role = _.find(itContractRoles, { id: right.roleId }),

                        right.edit = false;

                        notify.addSuccessMessage(right.user.fullName + " er knyttet i rollen");

                    }).error(function (result) {

                        //we successfully deleted the old entry, but didn't add a new one
                        //fuck

                        right.show = false;

                        notify.addErrorMessage('Fejl!');
                    });

                }).error(function (deleteResult) {

                    //couldn't delete the old entry, just reset select options
                    right.userForSelect = { id: right.user.id, text: right.user.fullName };
                    right.roleForSelect = right.roleId;

                    notify.addErrorMessage('Fejl!');
                });
            };

            $scope.rightSortBy = "roleName";
            $scope.rightSortReverse = false;
            $scope.rightSort = function (right) {
                switch ($scope.rightSortBy) {
                    case "roleName":
                        return right.role.name;
                    case "userName":
                        return right.user.name;
                    case "userEmail":
                        return right.user.email;
                    default:
                        return right.role.name;
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

        }]);

})(angular, app);