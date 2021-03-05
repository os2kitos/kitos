(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-system.usage.roles", {
            url: "/roles",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-roles.view.html",
            controller: "system.EditRoles",
            resolve: {
                itSystemRoles: ["$http", function ($http) {
                    return $http.get("odata/ItSystemRoles")
                        .then(function (result) {
                            return result.data.value;
                        });
                }],
                localItSystemRoles: ["localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                    localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.ItSystemRoles).getAll()],
                user: ["userService", function (userService) {
                    return userService.getUser().then(function (user) {
                        return user;
                    });
                }]
            }
        });
    }]);

    app.controller("system.EditRoles", ["$scope", "$http", "notify", "itSystemUsage", "itSystemRoles", "localItSystemRoles", "user", function ($scope, $http, notify, itSystemUsage, itSystemRoles, localItSystemRoles, user) {
        var usageId = itSystemUsage.id;

        $scope.activeItSystemRoles = localItSystemRoles;
        $scope.itSystemRoles = itSystemRoles;
        $scope.newRole = 1;
        $scope.orgId = user.currentOrganizationId;

        $scope.rights = [];
        _.each(itSystemUsage.rights, function (right: { roleId; role; show; user; userForSelect; roleForSelect; }) {
            right.role = _.find(itSystemRoles, { Id: right.roleId });
            right.show = true;

            var localRole: any = _.find($scope.activeItSystemRoles, { Id: right.roleId });

            if (!angular.isUndefined(localRole) && localRole.Description) {
                right.role.Description = localRole.Description;
            }

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

            $http.post("api/itSystemUsageRights/" + usageId + "?organizationId=" + user.currentOrganizationId, data)
                .then(function onSuccess(result) {
                    notify.addSuccessMessage(result.data.response.user.fullName + " er knyttet i rollen");

                    $scope.rights.push({
                        objectId: result.data.response.objectId,
                        roleId: result.data.response.roleId,
                        userId: result.data.response.userId,
                        user: result.data.response.user,
                        userForSelect: { id: result.data.response.userId, text: result.data.response.user.fullName },
                        roleForSelect: result.data.response.roleId,
                        role: _.find(localItSystemRoles, { Id: result.data.response.roleId }),
                        show: true
                    });

                    $scope.newRole = 1;
                    $scope.selectedUser = "";

                }, function onError(result) {

                    notify.addErrorMessage("Fejl!");
                });
        };

        $scope.deleteRight = function (right) {

            var rId = right.roleId;
            var uId = right.userId;

            $http.delete("api/itSystemUsageRights/" + usageId + "?rId=" + rId + "&uId=" + uId + "&organizationId=" + user.currentOrganizationId)
                .then(function onSuccess(result) {
                    right.show = false;
                    notify.addSuccessMessage("Rollen er slettet!");
                }, function onError(result) {

                    notify.addErrorMessage("Kunne ikke slette rollen!");
                });

        };

        $scope.updateRight = function (right) {
            if (!right.roleForSelect || !right.userForSelect) return;

            if (!$scope.checkIfRoleIsAvailable(right.roleForSelect)) {
                right.edit = false;
                return;
            }

            // old values
            var rIdOld = right.roleId;
            var uIdOld = right.userId;

            // new values
            var rIdNew = right.roleForSelect;
            var uIdNew = right.userForSelect.id;

            // if nothing was changed, just exit edit-mode
            if (rIdOld == rIdNew && uIdOld == uIdNew) {
                right.edit = false;
            }

            // otherwise, we should delete the old entry, then add a new one

            $http.delete("api/itSystemUsageRights/" + usageId + "?rId=" + rIdOld + "&uId=" + uIdOld + "&organizationId=" + user.currentOrganizationId)
                .then(function onSuccess(result) {

                    var data = {
                        "roleId": rIdNew,
                        "userId": uIdNew
                    };

                    $http.post("api/itSystemUsageRights/" + usageId + "?organizationId=" + user.currentOrganizationId, data)
                        .then(function onSuccess(result) {

                            right.roleId = result.data.response.roleId;
                            right.user = result.data.response.user;
                            right.userId = result.data.response.userId;

                            right.role = _.find(localItSystemRoles, { Id: right.roleId }),

                                right.edit = false;

                            notify.addSuccessMessage(right.user.fullName + " er knyttet i rollen");

                        }, function onError(result) {

                            // we successfully deleted the old entry, but didn't add a new one
                            right.show = false;

                            notify.addErrorMessage("Fejl!");
                        });

                }, function onError(result) {

                    // couldn't delete the old entry, just reset select options
                    right.userForSelect = { id: right.user.id, text: right.user.fullName };
                    right.roleForSelect = right.roleId;

                    notify.addErrorMessage("Fejl!");
                });
        };

        $scope.rightSortBy = "roleName";
        $scope.rightSortReverse = false;
        $scope.rightSort = function (right) {
            switch ($scope.rightSortBy) {
                case "roleName":
                    return right.role.Priority;
                case "userName":
                    return right.user.name;
                case "userEmail":
                    return right.user.email;
                default:
                    return right.role.Priority;
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

        $scope.checkIfRoleIsAvailable = function (roleId) {
            var foundSelectedInOptions = _.find($scope.activeItSystemRoles, function (option: any) { return option.Id === parseInt(roleId, 10) });
            return (foundSelectedInOptions);
        }

    }]);

})(angular, app);
