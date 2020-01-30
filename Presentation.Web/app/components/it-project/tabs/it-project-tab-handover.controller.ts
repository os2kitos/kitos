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
                        .then(function(rightResult) {
                            var rights = rightResult.data.response;

                            //get the role names
                            return $http.get("odata/ItProjectRoles?%24format=json&%24top=100&%24orderby=priority+desc&%24count=true")
                                .then(function(roleResult) {
                                    var roles: { Name }[] = roleResult.data.value;

                                    //the resulting map
                                    var users = {};
                                    _.each(rights, function(right: { userId; user; roleId; }) {

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
    ["$scope", "$http", "$stateParams", "dateFilter", "handover", "usersWithRoles",
        function ($scope, $http, $stateParams, dateFilter, handover, usersWithRoles) {
            $scope.handover = handover;
            $scope.usersWithRoles = usersWithRoles;
            $scope.autosaveUrl = "api/handover/" + $stateParams.id;
            $scope.participants = _.map(handover.participants, function(user: { id }) {
                return user.id.toString();
            });

            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            $scope.select2Options = {
                dropdownAutoWidth: true
            };

            $scope.$watch("participants", function (newValue, oldValue) {
                if (newValue.length > oldValue.length) {
                    // something was added
                    var addId = _.difference(newValue, oldValue);
                    if (!_.isUndefined(addId)) {
                        for (var j = 0; j < addId.length; j++) {
                            $http.post($scope.autosaveUrl + "?participantId=" + addId[j]);
                        }
                    }
                } else if (newValue.length < oldValue.length) {
                    // something was removed
                    var removeId = _.difference(oldValue, newValue);
                    if (!_.isUndefined(removeId)) {
                        for (var i = 0; i < removeId.length; i++) {
                            $http.delete($scope.autosaveUrl + "?participantId=" + removeId[i]);
                        }
                    }
                }
            });
        }]);
})(angular, app);
