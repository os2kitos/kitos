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

                            return $http.get('api/user?orgId=' + user.currentOrganizationId).then(function (result) {
                                return result.data.response;
                            });
                        }
                    ]
                }
            });
        }
    ]);

    app.controller('org.UserCtrl', [
        '$scope', '$http', '$state', '$modal', '$q', 'notify', 'users',
        function ($scope, $http, $state, $modal, $q, notify, users) {
            $scope.users = users;

            $scope.toggleStatus = function (user) {
                user.isLocked = !user.isLocked;
                var success = user.isLocked ? user.name + " er låst" : user.name + " er låst op";
                updateUser(user, success).then(//success
                                function (successMessage) {
                                    notify.addSuccessMessage(successMessage);
                                },
                                //failure
                                function (errorMessage) {
                                    notify.addErrorMessage(errorMessage);
                                },
                                //update
                                function (updateMessage) {
                                    notify.addInfoMessage(updateMessage);
                                });
            };

            $scope.editUser = function (user) {

                var modal = $modal.open({
                    // fade in instead of slide from top, fixes strange cursor placement in IE
                    // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                    windowClass: 'modal fade in',
                    templateUrl: 'partials/org/user/org-edituser-modal.html',
                    controller: ['$scope', '$modalInstance', 'notify', 'autofocus', function ($modalScope, $modalInstance, modalnotify, autofocus) {
                        autofocus();
                        $modalScope.busy = false;
                        $modalScope.name = user.name;
                        $modalScope.email = user.email;
                        $modalScope.repeatEmail = user.email;
                        $modalScope.ok = function () {
                            $modalScope.busy = true;
                            user.name = $modalScope.name;
                            user.email = $modalScope.email;
                            updateUser(user, user.name + " er ændret.", true).then(
                                //success
                                function (successMessage) {
                                    modalnotify.addSuccessMessage(successMessage);
                                    $modalInstance.close();
                                },
                                //failure
                                function (errorMessage) {
                                    modalnotify.addErrorMessage(errorMessage);
                                    $modalInstance.close();
                                },
                                //update
                                function (updateMessage) {
                                    modalnotify.addInfoMessage(updateMessage);
                                });
                        };
                        $modalScope.cancel = function () {
                            $modalInstance.close();
                        };
                    }]
                });

                modal.result.then(
                    function () {
                        reload();
                    });
            };

            function reload() {
                $state.go('.', null, { reload: true });
            }

            function updateUser(user, successmessage, showNotify) {

                var deferred = $q.defer();

                setTimeout(function () {
                    if (showNotify)
                        deferred.notify('Ændrer...');
                    $http({ method: 'PATCH', url: "api/user/" + user.id, data: user, handleBusy: true }).success(function (result) {
                        deferred.resolve(successmessage);
                    }).error(function (result) {
                        deferred.reject("Fejl! " + user.name + " kunne ikke ændres!");
                    });
                }, 0);

                return deferred.promise;
            }
        }
    ]);
})(angular, app);
