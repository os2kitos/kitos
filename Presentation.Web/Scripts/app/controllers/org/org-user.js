(function (ng, app) {
    app.config([
        '$stateProvider', function ($stateProvider) {
            $stateProvider.state('organization.user', {
                url: '/user',
                templateUrl: 'partials/org/user/org-user.html',
                controller: 'org.UserCtrl',
                resolve: {
                    user: ['userService', function (userService) {
                        return userService.getUser();
                    }],
                    users: [
                        '$http', 'user', function ($http, user) {

                            return $http.get('api/user?orgId=' + user.currentOrganizationId).then(function(result) {
                                return result.data.response;
                            });
                        }
                    ]
                }
            });
        }
    ]);

    app.controller('org.UserCtrl', [
        '$scope', '$http', '$state', '$modal', 'notify', 'users',
        function ($scope, $http, $state, $modal, notify, users) {
            $scope.users = users;

            $scope.toggleStatus = function (user) {
                user.isLocked = !user.isLocked;
                updateUser(user);
            };

            $scope.editUser = function (user) {

                var modal = $modal.open({
                    // fade in instead of slide from top, fixes strange cursor placement in IE
                    // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                    windowClass: 'modal fade in',
                    templateUrl: 'partials/org/user/org-edituser-modal.html',
                    controller: ['$scope', '$modalInstance', 'autofocus', function ($modalScope, $modalInstance, autofocus) {
                        autofocus();
                        $modalScope.name = user.name;
                        $modalScope.email = user.email;
                        $modalScope.repeatEmail = user.email;
                        $modalScope.ok = function (){

                            user.name = $modalScope.name;
                            user.email = $modalScope.email;
                            updateUser(user);
                            $modalInstance.close();
                        };
                        $modalScope.cancel = function () {
                            $modalInstance.close();
                        };
                    }]
                });
            };

            function reload() {
                $state.go('.', null, { reload: true });
            }

            function updateUser(user) {
                $http({ method: 'PATCH', url: "api/user/" + user.id, data: user }).success(function (result) {
                    notify.addSuccessMessage(user.name + " er ændret.");
                    reload();
                }).error(function (result) {
                    notify.addErrorMessage("Fejl! " + user.name + " kunne ikke ændres!");
                });
            }
        }
    ]);
})(angular, app);
