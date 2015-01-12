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
        '$scope', '$http', '$state', 'notify', 'users',
        function ($scope, $http, $state, notify, users) {
            $scope.users = users;
            $scope.createUser = function () { };

            $scope.toggleStatus = function (user) {
                user.isLocked = !user.isLocked;
                $http({ method: 'PATCH', url: "api/user/" + user.id, data: user }).success(function (result) {
                    notify.addSuccessMessage(user.name + " er ændret.");
                    reload();
                }).error(function (result) {
                    notify.addErrorMessage("Fejl! " + user.name + " kunne ikke ændres!");
                });
            };

            function reload() {
                $state.go('.', null, { reload: true });
            }
        }
    ]);
})(angular, app);
