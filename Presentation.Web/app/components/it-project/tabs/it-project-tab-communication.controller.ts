(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-project.edit.communication", {
            url: "/communication",
            templateUrl: "app/components/it-project/tabs/it-project-tab-communication.view.html",
            controller: "project.EditCommunicationCtrl",
            resolve: {
                comms: ["$http", "$stateParams", function ($http, $stateParams) {
                    return $http.get("api/communication/" + $stateParams.id + "?project=true").then(function (result) {
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

                                        var role: { Name } = _.find(roles, { Id: right.roleId });

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

    app.controller("project.EditCommunicationCtrl",
        ["$scope", "$http", "notify", "$timeout", "$state", "$stateParams", "comms", "usersWithRoles", "user", "entityMapper", 
            function ($scope, $http, notify, $timeout, $state, $stateParams, comms, usersWithRoles, user, entityMapper) {
            $scope.comms = comms;
            $scope.usersWithRoles = entityMapper.mapApiResponseToSelect2ViewModel(usersWithRoles);
            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            $scope.comm = {
                itProjectId: $stateParams.id
            };

            $scope.save = function () {
                $scope.$broadcast("show-errors-check-validity");

                if ($scope.commForm.$invalid) { return; }

                var dueDate = moment($scope.comm.dueDate, "DD-MM-YYYY");
                if (dueDate.isValid()) {
                    $scope.comm.dueDate = dueDate.format("YYYY-MM-DD");
                } else {
                    $scope.comm.dueDate = null;
                }

                var payload = {
                    dueDate: $scope.comm.dueDate,
                    itProjectId: $scope.comm.itProjectId,
                    responsibleUserId: $scope.comm.responsibleUserId.id,
                    media: $scope.comm.media,
                    message: $scope.comm.message,
                    purpose: $scope.comm.purpose,
                    targetAudiance: $scope.comm.targetAudiance
                }

                $http.post(`api/communication?organizationId=${user.currentOrganizationId}`, payload).finally(reload);
            };

            $scope.delete = function (id) {
                $http.delete("api/communication/" + id + "?organizationId=" + user.currentOrganizationId).finally(reload);
            };

            $scope.checkDate = (value) => {
                var date = moment(value, "DD-MM-YYYY");
                $scope.DateFailed = false;
                if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig.");
                    $scope.DateFailed = true;
                }
            }
            // work around for $state.reload() not updating scope
            // https://github.com/angular-ui/ui-router/issues/582
            function reload() {
                return $state.transitionTo($state.current, $stateParams, {
                    reload: true
                }).then(function () {
                    $scope.hideContent = true;
                    return $timeout(function () {
                        return $scope.hideContent = false;
                    }, 1);
                });
            };
        }]);
})(angular, app);
