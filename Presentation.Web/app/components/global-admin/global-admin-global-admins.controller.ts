((ng, app) => {
    app.config([
        '$stateProvider', $stateProvider => {
            $stateProvider.state('global-admin.global-users', {
                url: '/global-admins',
                templateUrl: 'app/components/global-admin/global-admin-global-admins.view.html',
                controller: 'globalAdmin.globalAdminsCtrl',
                authRoles: ['GlobalAdmin']
            });
        }
    ]);

    app.controller('globalAdmin.globalAdminsCtrl', [
        '$rootScope', '$scope', '$http', 'notify', ($rootScope, $scope, $http, notify) => {
            $rootScope.page.title = 'Globale administratorer';

            $scope.globalAdmins = [];

            function pushAdmin(user) {
                var admin = {
                    show: true,
                    user: user,
                    userForSelect: {
                        id: user.id,
                        text: user.name,
                        user: user
                    }
                };

                $scope.globalAdmins.push(admin);
            }

            $http.get("api/globaladmin").success(result => {
                _.each(result.response, pushAdmin);
            });

            function newGlobalAdmin() {
                if (!$scope.newUser) return;

                let user = $scope.newUser;
                var uId = user.id;

                if (!uId) return;

                var data = {
                    userId: uId
                };

                var msg = notify.addInfoMessage("Arbejder ...", false);

                $http.post("api/globaladmin", data, { handleBusy: true }).success(result => {
                    msg.toSuccessMessage(user.text + " er blevet global administrator");
                    $scope.newUser = null;

                    pushAdmin(result.response);

                }).error(result => {
                    msg.toErrorMessage("Kunne ikke gøre " + user.text + " til global administrator");
                });
            }

            $scope.$watch("newUser", function(newVal, oldVal) {
                if (newVal === oldVal || !newVal) return;

                newGlobalAdmin();
            });

            $scope.deleteGlobalAdmin = function(admin) {

                var uId = admin.user.id;

                var msg = notify.addInfoMessage("Arbejder ...", false);
                $http.delete("api/globaladmin?userId=" + uId).success(function(deleteResult) {
                    admin.show = false;
                    msg.toSuccessMessage(admin.user.name + " er ikke længere global administrator");

                }).error(function(deleteResult) {
                    msg.toErrorMessage("Kunne ikke fjerne " + admin.user.name + " som global administrator");
                });
            };

            $scope.updateGlobalAdmin = function(admin) {

                if (!admin.userForSelect) return;

                var user = admin.userForSelect;

                //old values
                var uIdOld = admin.user.id;

                //new values
                var uIdNew = user.id;

                if (!uIdNew) return;

                //if nothing was changed, just exit edit-mode
                if (uIdOld == uIdNew) {
                    admin.edit = false;
                    return;
                }

                //otherwise, we should delete the old entry, then add a new one
                var msg = notify.addInfoMessage("Arbejder ...", false);

                $http.delete("api/globaladmin?userId=" + uIdOld, { handleBusy: true }).success(function(deleteResult) {
                    var data = {
                        userId: uIdNew
                    };

                    $http.post("api/globaladmin", data, { handleBusy: true }).success(function(result) {
                        msg.toSuccessMessage(user.text + " er blevet global administrator");
                        $scope.newUser = null;

                        var newUser = result.response;

                        admin.user = newUser;
                        admin.userForSelect = {
                            id: newUser.id,
                            text: newUser.text,
                            user: newUser
                        };

                        admin.edit = false;

                    }).error(function(result) {
                        //we successfully deleted the old entry, but didn't add a new one
                        admin.show = false;

                        msg.toErrorMessage("Kunne ikke gøre " + user.text + " til global administrator");
                    });
                }).error(function(deleteResult) {
                    //couldn't delete the old entry, just reset select options
                    admin.userForSelect = { id: admin.user.id, text: admin.user.name };

                    msg.toErrorMessage("Fejl!");
                });
            };

            $scope.adminSortBy = "userName";
            $scope.adminSortReverse = false;
            $scope.adminSort = function(admin) {
                switch ($scope.adminSortBy) {
                case "userName":
                    return admin.user.name;
                case "userEmail":
                    return admin.user.email;
                default:
                    return admin.user.name;
                }
            };

            $scope.adminSortChange = function(val) {
                if ($scope.adminSortBy == val) {
                    $scope.adminSortReverse = !$scope.adminSortReverse;
                } else {
                    $scope.adminSortReverse = false;
                }

                $scope.adminSortBy = val;
            };
        }
    ]);
})(angular, app);
