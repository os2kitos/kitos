(function(ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('global-admin.local-users', {
            url: '/local-admins',
            templateUrl: 'partials/global-admin/local-admins.html',
            controller: 'globalAdmin.localAdminsCtrl',
            authRoles: ['GlobalAdmin']
        });
    }]);

    app.controller('globalAdmin.localAdminsCtrl', [
        '$rootScope', '$scope', '$http', 'notify', function($rootScope, $scope, $http, notify) {
            $rootScope.page.title = 'Lokal administratorer';

            $scope.organizations = {};
            $http.get("api/organization").success(function(result) {
                _.each(result.response, function(org) {
                    $scope.organizations[org.id] = org;
                });
            });

            var localAdminRole = null;
            $http.get("api/adminrole?getLocalAdminRole=true").success(function(result) {
                localAdminRole = result.response;
            });

            $scope.adminRights = [];

            function pushRight(right) {
                right.show = true;

                right.orgForSelect = right.objectId;
                right.userForSelect = {
                    id: right.user.id,
                    text: right.user.name,
                    user: right.user
                };

                $scope.adminRights.push(right);
            }

            $http.get("api/adminrights").success(function(result) {
                _.each(result.response, pushRight);
            });

            function newLocalAdmin() {
                if (!(localAdminRole && $scope.newOrg && $scope.newUser)) return;

                var user = $scope.newUser;
                var uId = user.id;
                var oId = parseInt($scope.newOrg);
                var orgName = $scope.organizations[oId].name;

                var rId = localAdminRole.id;

                if (!(uId && oId && rId)) return;

                var data = {
                    userId: uId,
                    roleId: rId,
                };

                console.log(data);
                var msg = notify.addInfoMessage("Arbejder ...", false);

                $http.post("api/adminrights/" + oId, data, { handleBusy: true }).success(function(result) {
                    msg.toSuccessMessage(user.text + " er blevet lokal administrator for " + orgName);
                    $scope.newUser = null;
                    $scope.newOrg = null;

                    pushRight(result.response);

                }).error(function(result) {
                    msg.toErrorMessage("Kunne ikke gøre " + user.text + " til lokal administrator for " + orgName);
                });
            }

            $scope.$watch("newUser", function(newVal, oldVal) {
                if (newVal === oldVal || !newVal) return;

                newLocalAdmin();
            });

            $scope.$watch("newOrg", function(newVal, oldVal) {
                if (newVal === oldVal || !newVal) return;

                newLocalAdmin();
            });

            $scope.deleteLocalAdmin = function(right) {

                var oId = right.objectId;
                var rId = right.roleId;
                var uId = right.userId;

                var msg = notify.addInfoMessage("Arbejder ...", false);
                $http.delete("api/adminrights/" + oId + "?rId=" + rId + "&uId=" + uId).success(function(deleteResult) {
                    right.show = false;
                    msg.toSuccessMessage(right.user.name + " er ikke længere lokal administrator");
                }).error(function(deleteResult) {

                    msg.toErrorMessage("Kunne ikke fjerne " + right.user.name + " som lokal administrator");
                });
            };

            $scope.updateLocalAdmin = function(right) {

                if (!right.orgForSelect || !right.userForSelect || !localAdminRole) return;

                //old values
                var oIdOld = right.objectId;
                var uIdOld = right.userId;

                //new values
                var oIdNew = parseInt(right.orgForSelect);
                var uIdNew = right.userForSelect.id;

                //if nothing was changed, just exit edit-mode
                if (oIdOld == oIdNew && uIdOld == uIdNew) {
                    right.edit = false;
                    return;
                }

                //otherwise, we should delete the old entry, then add a new one

                var rId = localAdminRole.id;

                var msg = notify.addInfoMessage("Arbejder ...", false);

                $http.delete("api/adminrights/" + oIdOld + "?rId=" + rId + "&uId=" + uIdOld, { handleBusy: true }).success(function(deleteResult) {

                    var newData = {
                        userId: uIdNew,
                        roleId: rId,
                    };

                    var orgName = $scope.organizations[oIdNew].name;

                    $http.post("api/adminrights/" + oIdNew, newData).success(function(result) {

                        right.roleId = result.response.roleId;
                        right.user = result.response.user;
                        right.userId = result.response.userId;

                        right.edit = false;

                        msg.toSuccessMessage(right.user.name + " er blevet lokal administrator for " + orgName);

                    }).error(function(result) {

                        //we successfully deleted the old entry, but didn't add a new one
                        //fuck

                        right.show = false;

                        msg.toErrorMessage("Kunne ikke gøre " + right.userForSelect.text + " til lokal administrator for " + orgName);
                    });

                }).error(function(deleteResult) {

                    //couldn't delete the old entry, just reset select options
                    right.userForSelect = { id: right.user.id, text: right.user.name };
                    right.orgForSelect = right.objectId;

                    msg.toErrorMessage("Fejl!");
                });
            };
            $scope.rightSortBy = "userName";
            $scope.rightSortReverse = false;
            $scope.rightSort = function(right) {
                switch ($scope.rightSortBy) {
                case "orgName":
                    return $scope.organizations[right.objectId].name;
                case "userName":
                    return right.user.name;
                case "userEmail":
                    return right.user.email;
                default:
                    return $scope.organizations[right.objectId].name;
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
        }
    ]);
})(angular, app);
