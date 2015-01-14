(function (ng, app) {
    app.config([
        '$stateProvider', function ($stateProvider) {
            $stateProvider.state('organization.user', {
                url: '/user',
                templateUrl: 'partials/org/user/org-user.html',
                controller: 'org.UserCtrl',
                resolve: {
                    user: [
                        'userService', function (userService) {
                            return userService.getUser();
                        }
                    ]
                }
            });
        }
    ]);

    app.controller('org.UserCtrl', [
            '$scope', '$http', '$state', '$modal', '$q', 'notify', 'user',
            function ($scope, $http, $state, $modal, $q, notify, user) {

                $scope.pagination = {
                    search: '',
                    skip: 0,
                    take: 10,
                    orderBy: 'Name'
                };

                $scope.$watchCollection('pagination', function (newVal, oldVal) {
                    loadUsers();
                });

                function loadUsers() {
                    var deferred = $q.defer();

                    var url = 'api/user?orgId=' + user.currentOrganizationId;
                    url += '&usePaging';
                    url += '&skip=' + $scope.pagination.skip;
                    url += '&take=' + $scope.pagination.take;

                    if ($scope.pagination.orderBy) {
                        url += '&orderBy=' + $scope.pagination.orderBy;
                        if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                    }

                    if ($scope.pagination.search) url += '&q=' + $scope.pagination.search;
                    else url += "&q=";

                    $scope.users = [];
                    $http.get(url).success(function (result, status, headers) {

                        var paginationHeader = JSON.parse(headers('X-Pagination'));
                        $scope.totalCount = paginationHeader.TotalCount;

                        ////Set current user's writeaccessrights for each other user in the returned list
                        setCanEdit(result.response).then(function (canEditResult) {
                            $scope.users = canEditResult;
                            deferred.resolve();
                        });

                    }).error(function () {
                        notify.addErrorMessage("Kunne ikke hente brugere!");
                        deferred.reject();
                    });

                    return deferred.promise;
                }

                function setCanEdit(canEditUsers) {
                    return $q.all(_.map(canEditUsers, function (iteratee) {
                        var deferred = $q.defer();

                        setTimeout(function () {
                            $http.get("api/user/" + iteratee.id + "?hasWriteAccess")
                            .success(function (result) {
                                iteratee.canBeEdited = result.response;
                                deferred.resolve(iteratee);
                            })
                            .error(function (result) {
                                iteratee.canBeEdited = false;
                                deferred.reject(result);
                            }
                            );
                        }, 0);

                        return deferred.promise;
                    }));
                }

                $scope.toggleStatus = function (userToToggle) {
                    userToToggle.isLocked = !userToToggle.isLocked;
                    var success = userToToggle.isLocked ? userToToggle.name + " er låst" : userToToggle.name + " er låst op";
                    updateUser(userToToggle, success).then( //success
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
                }

                $scope.editUser = function (userToEdit) {

                    var modal = $modal.open({
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: 'modal fade in',
                        templateUrl: 'partials/org/user/org-edituser-modal.html',
                        controller: [
                            '$scope', '$modalInstance', 'notify', 'autofocus', function ($modalScope, $modalInstance, modalnotify, autofocus) {
                                autofocus();
                                $modalScope.busy = false;
                                $modalScope.name = userToEdit.name;
                                $modalScope.email = userToEdit.email;
                                $modalScope.repeatEmail = userToEdit.email;
                                $modalScope.ok = function () {
                                    $modalScope.busy = true;
                                    userToEdit.name = $modalScope.name;
                                    userToEdit.email = $modalScope.email;
                                    updateUser(userToEdit, userToEdit.name + " er ændret.", true).then(
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
                            }
                        ]
                    });

                    modal.result.then(
                        function () {
                            reload();
                        });
                }

                $scope.sendAdvis = function (userToAdvis, reminder) {
                    var params = {};
                    var type;
                    if (reminder) {
                        params.sendReminder = true;
                        type = "påmindelse";
                    } else {
                        params.sendAdvis = true;
                        type = "advis";
                    }

                    var msg = notify.addInfoMessage("Sender " + type + " til " + userToAdvis.email, false);
                    $http.post("api/user", userToAdvis, { handleBusy: true, params: params })
                        .success(function (result) {
                            msg.toSuccessMessage("Advis sendt til " + userToAdvis.email);
                        })
                        .error(function (result) {
                            msg.toErrorMessage("Kunne ikke sende " + type + "!");
                        })
                        .then(function () {
                            reload();
                        });
                }

                function reload() {
                    $state.go('.', null, { reload: true });
                }

                function updateUser(userToUpdate, successmessage, showNotify) {

                    var deferred = $q.defer();

                    setTimeout(function () {
                        if (showNotify)
                            deferred.notify('Ændrer...');
                        $http({ method: 'PATCH', url: "api/user/" + userToUpdate.id, data: userToUpdate, handleBusy: true })
                            .success(function (result) {
                                deferred.resolve(successmessage);
                            })
                            .error(function (result) {
                                deferred.reject("Fejl! " + userToUpdate.name + " kunne ikke ændres!");
                            });
                    }, 0);

                    return deferred.promise;
                }
            }
    ]);
})(angular, app);
