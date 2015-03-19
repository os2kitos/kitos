(function (ng, app) {
    app.config([
        '$stateProvider', function ($stateProvider) {
            $stateProvider.state('organization.user', {
                url: '/user:lastModule',
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
            '$scope', '$http', '$state', '$modal', '$q', '$stateParams', 'notify', 'user',
            function ($scope, $http, $state, $modal, $q, $stateParams, notify, user) {

                $scope.rootUnitName = user.currentOrganization.root.name;

                function isLocalAdmin(selectedUser) {

                    var uId = selectedUser.id;
                    var oId = user.currentOrganizationId;

                    return $http.get("api/adminrights/?roleName=LocalAdmin&userId=" + uId + "&organizationId=" + oId + "&orgRightsForUserWithRole=").success(function (result) {
                        return result.response;
                    }).error(function (error) {
                        notify.addErrorMessage("Fejl. Noget gik galt!");
                    });
                }

                function loadUsers() {
                    var deferred = $q.defer();

                    var url = 'api/user?overview';
                    url += '&orgId=' + user.currentOrganizationId;
                    url += '&skip=' + $scope.pagination.skip;
                    url += '&take=' + $scope.pagination.take;

                    if ($scope.pagination.orderBy) {
                        url += '&orderBy=' + $scope.pagination.orderBy;
                        if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                    }

                    if ($scope.pagination.search)
                        url += '&q=' + $scope.pagination.search;
                    else
                        url += "&q=";

                    $scope.users = [];
                    $http.get(url).success(function (result, status, headers) {

                        var paginationHeader = JSON.parse(headers('X-Pagination'));
                        $scope.totalCount = paginationHeader.TotalCount;

                        $scope.users = result.response;
                        deferred.resolve();

                        ////Set current user's writeaccessrights for each other user in the returned list
                        //setCanEdit(result.response).then(function (canEditResult) {
                        //    $scope.users = canEditResult;
                        //    deferred.resolve();
                        //});

                    }).error(function () {
                        notify.addErrorMessage("Kunne ikke hente brugere!");
                        deferred.reject();
                    });

                    return deferred.promise;
                }

                //Goes through a collection of users, and for each user sets canBeEdited flag
                //Returns a flattened promise, that resolves when all users in the collection has been resolved
                function setCanEdit(userCollection) {
                    return $q.all(_.map(userCollection, function (iteratee) {
                        var deferred = $q.defer();

                        iteratee.adminRights = _.findWhere(iteratee.adminRights, { roleName: "Medarbejder", organizationId: user.currentOrganizationId });

                        setTimeout(function () {
                            $http.get("api/user/" + iteratee.id + "?hasWriteAccess" + '&organizationId=' + user.currentOrganizationId)
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

                function updateUser(userToUpdate, successmessage, showNotify) {

                    var deferred = $q.defer();

                    setTimeout(function () {
                        if (showNotify)
                            deferred.notify('Ændrer...');
                        $http({ method: 'PATCH', url: "api/user/" + userToUpdate.id + "?organizationId=" + user.currentOrganizationId, data: userToUpdate, handleBusy: true })
                            .success(function(result) {
                                deferred.resolve(successmessage);
                            })
                            .error(function(result, status) {
                                if (status === 409) {
                                    deferred.reject("Fejl! Kan ikke ændre Email for " + userToUpdate.fullName + " da den allerede findes!");
                                    reload();
                                } else {
                                    deferred.reject("Fejl! " + userToUpdate.fullName + " kunne ikke ændres!");
                                }
                            });
                    }, 0);

                    return deferred.promise;
                }

                function deleteOrgRoleAction(u) {
                    var uId = u.id;

                    var msg = notify.addInfoMessage("Arbejder ...", false);

                    $http.delete("api/adminrights/?orgId=" + user.currentOrganizationId + "&userId=" + uId + "&byOrganization=").success(function (deleteResult) {
                        msg.toSuccessMessage(u.name + " " + u.lastName + " er ikke længere tilknyttet organisationen");
                        reload();
                    }).error(function (deleteResult) {
                        msg.toErrorMessage("Kunne ikke fjerne " + u.name + " " + u.lastName + " fra organisationen");
                    });
                }

                function reload() {
                    $state.go('.', { lastModule: $scope.chosenModule }, { reload: true });
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

                //remove a users organizationRole - thereby removing their readaccess for this organization
                $scope.deleteOrgRole = function (u) {
                    isLocalAdmin(u).then(function (response) {
                        if (response.data.response === true) {
                            var confirmBox = confirm(u.name + " " + u.lastName + " er også lokaladministrator!\n\nVil du fortsat fjerne tilknytning?");
                            if (confirmBox) {
                                deleteOrgRoleAction(u);
                            } else {
                                notify.addInfoMessage("Handling afbrudt!");
                            }
                        } else {
                            deleteOrgRoleAction(u);
                        }
                    });
                };

                $scope.getRightsForModule = function (chosenModule) {
                    //return a flat promise, that fullfills when all rights have been retrieved
                    return $q.all(_.map($scope.users, function (iteratee) {
                        var deferred = $q.defer();

                        setTimeout(function () {
                            var httpUrl = 'api/';

                            switch (chosenModule) {
                                //Choose Modul selected
                                case '0':
                                    iteratee.rights = '';
                                    return deferred.resolve();
                                //Organisation selected
                                case '1':
                                    httpUrl += 'organizationunitrights?orgId=' + user.currentOrganizationId;
                                    break;
                                //ITProjects selected
                                case '2':
                                    httpUrl += 'itprojectrights?';
                                    break;
                                //ITSystems selected
                                case '3':
                                    httpUrl += 'itsystemusagerights?';
                                    break;
                                //ITContracts selected
                                case '4':
                                    httpUrl += 'itcontractrights?';
                                    break;
                            }

                            httpUrl += '&userId=' + iteratee.id;
                            return $http.get(httpUrl, { handleBusy: true })
                                .success(function (result) {
                                    iteratee.rights = result.response;
                                    deferred.resolve();
                                })
                                .error(function (result) {
                                    deferred.reject();
                                });
                        }, 0);

                        return deferred.promise;
                    }));
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
                                $modalScope.lastName = userToEdit.lastName;
                                $modalScope.phoneNumber = userToEdit.phoneNumber;
                                $modalScope.ok = function () {
                                    $modalScope.busy = true;
                                    userToEdit.name = $modalScope.name;
                                    userToEdit.email = $modalScope.email;
                                    userToEdit.lastName = $modalScope.lastName;
                                    userToEdit.phoneNumber = $modalScope.phoneNumber;
                                    var msg = notify.addInfoMessage("Ændrer");
                                    updateUser(userToEdit, userToEdit.name + " er ændret.", true).then(
                                        //success
                                        function (successMessage) {
                                            msg.toSuccessMessage(successMessage);
                                            $modalInstance.close();
                                            reload();
                                        },
                                        //failure
                                        function (errorMessage) {
                                            msg.toErrorMessage(errorMessage);
                                            $modalInstance.close();
                                        },
                                        //update
                                        function (updateMessage) {
                                            msg.toInfoMessage(updateMessage);
                                        });
                                };
                                $modalScope.cancel = function () {
                                    $modalInstance.close();
                                };
                            }
                        ]
                    });

                    modal.result.then(function () { });
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
                    params.organizationId = user.currentOrganizationId;

                    var msg = notify.addInfoMessage("Sender " + type + " til " + userToAdvis.email, false);
                    $http.post("api/user", userToAdvis, { handleBusy: true, params: params })
                        .success(function (result) {
                            userToAdvis.lastAdvisDate = result.response.lastAdvisDate;
                            msg.toSuccessMessage("Advis sendt til " + userToAdvis.email);
                        })
                        .error(function (result) {
                            msg.toErrorMessage("Kunne ikke sende " + type + "!");
                        })
                        .then(function () { });
                }
                
                $scope.currentUser = user;

                $scope.pagination = {
                    search: '',
                    skip: 0,
                    take: 10,
                    orderBy: 'Name'
                };

                $scope.csvUrl = 'api/user/?csv&orgId=' + user.currentOrganizationId;

                //set or re-set the chosen module based on state
                if ($stateParams.lastModule)
                    $scope.chosenModule = $stateParams.lastModule;
                else
                    $scope.chosenModule = '0';
                
                $scope.$watchCollection('pagination', function (newVal, oldVal) {
                    loadUsers().then(function() {
                        $scope.getRightsForModule($scope.chosenModule).then(function() {
                            
                        });
                    });
                });
            }
    ]);
})(angular, app);
