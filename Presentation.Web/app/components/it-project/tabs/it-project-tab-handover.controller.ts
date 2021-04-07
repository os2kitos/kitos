(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-project.edit.handover", {
            url: "/handover",
            templateUrl: "app/components/it-project/tabs/it-project-tab-handover.view.html",
            controller: "project.EditHandoverCtrl",
            resolve: {
                handover: ["$http", "$stateParams", function ($http, $stateParams) {
                    var projectId = $stateParams.id;
                    return $http.get("api/handover/" + projectId)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                //returns a map with those users who have a role in this project.
                //the names of the roles is saved in user.roleNames
                usersWithRoles: ["$http", "$stateParams", function ($http, $stateParams) {

                    //get the rights of the projects
                    return $http.get("api/itprojectright/" + $stateParams.id)
                        .then(function (rightResult) {
                            var rights = rightResult.data.response;

                            //get the role names
                            return $http.get("odata/ItProjectRoles?%24format=json&%24top=100&%24orderby=priority+desc&%24count=true")
                                .then(function (roleResult) {
                                    var roles = roleResult.data.value;

                                    //the resulting map
                                    var users = {};
                                    _.each(rights, function (right: { userId; user; roleId; }) {

                                        //use the user from the map if possible
                                        var user = users[right.userId] || right.user;

                                        var role = _.find(roles, { Id: right.roleId });

                                        var roleNames = user.roleNames || [];
                                        roleNames.push(role.Name);
                                        user.roleNames = roleNames;

                                        users[right.userId] = user;
                                    });

                                    return users;
                                });
                        });
                }]
            }
        });
    }]);

    app.controller("project.EditHandoverCtrl",
        ["$scope", "$http", "$stateParams", "notify", "handover", "usersWithRoles", "entityMapper",
            function ($scope, $http, $stateParams, notify, handover, usersWithRoles, entityMapper) {
                $scope.handover = handover;

                $scope.availableUsers = entityMapper.mapApiResponseToSelect2ViewModel(usersWithRoles);
                $scope.selectedUsers = entityMapper.mapApiResponseToSelect2ViewModel(handover.participants);

                $scope.autosaveUrl = "api/handover/" + $stateParams.id;

                $scope.datepickerOptions = {
                    format: "dd-MM-yyyy",
                    parseFormats: ["yyyy-MM-dd"]
                };

                $scope.select2Options = {
                    dropdownAutoWidth: true
                };

                $scope.$watch("participants", function (newValue, oldValue) {
                    if (_.isUndefined(newValue) || _.isUndefined(oldValue)) {
                        return;
                    }
                    if (newValue.length > oldValue.length) {
                        // something was added
                        var toAdd: any = _.difference(newValue, oldValue);
                        if (!_.isUndefined(toAdd)) {
                            for (var j = 0; j < toAdd.length; j++) {
                                var msg = notify.addInfoMessage("Gemmer...", false);
                                $http.post($scope.autosaveUrl + "?participantId=" + toAdd[j].id)
                                    .then(function onSuccess(result) {
                                        msg.toSuccessMessage("Feltet er opdateret.");
                                    }, function onError(error) {
                                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                    });
                            }
                        }
                    } else if (newValue.length < oldValue.length) {
                        // something was removed
                        var toRemove: any = _.difference(oldValue, newValue);
                        if (!_.isUndefined(toRemove)) {
                            for (var i = 0; i < toRemove.length; i++) {
                                var msg = notify.addInfoMessage("Gemmer...", false);
                                $http.delete($scope.autosaveUrl + "?participantId=" + toRemove[i].id)
                                    .then(function onSuccess(result) {
                                        msg.toSuccessMessage("Feltet er opdateret.");
                                    }, function onError(error) {
                                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                    });
                            }
                        }
                    }
                });
            }]);
})(angular, app);
