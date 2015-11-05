﻿(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.handover', {
            url: '/handover',
            templateUrl: 'partials/it-project/tab-handover.html',
            controller: 'project.EditHandoverCtrl',
            resolve: {
                handover: ['$http', '$stateParams', function ($http, $stateParams) {
                    var projectId = $stateParams.id;
                    return $http.get('api/handover/' + projectId)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                //returns a map with those users who have a role in this project.
                //the names of the roles is saved in user.roleNames
                usersWithRoles: ['$http', '$stateParams', function ($http, $stateParams) {

                    //get the rights of the projects
                    return $http.get('api/itprojectrights/' + $stateParams.id)
                        .then(function(rightResult) {
                            var rights = rightResult.data.response;

                            //get the role names
                            return $http.get('api/itprojectrole/')
                                .then(function(roleResult) {
                                    var roles: { name }[] = roleResult.data.response;

                                    //the resulting map
                                    var users = {};
                                    _.each(rights, function(right: { userId; user; roleId; }) {

                                        //use the user from the map if possible
                                        var user = users[right.userId] || right.user;

                                        var role = _.findWhere(roles, { id: right.roleId });

                                        var roleNames = user.roleNames || [];
                                        roleNames.push(role.name);
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

    app.controller('project.EditHandoverCtrl',
    ['$scope', '$http', '$stateParams', 'dateFilter', 'handover', 'usersWithRoles',
        function ($scope, $http, $stateParams, dateFilter, handover, usersWithRoles) {
            $scope.handover = handover;
            $scope.usersWithRoles = usersWithRoles;
            $scope.autosaveUrl = 'api/handover/' + $stateParams.id;
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

            $scope.$watch('participants', function (newValue, oldValue) {
                if (newValue.length > oldValue.length) {
                    // something was added
                    var addId = _.difference(newValue, oldValue);
                    $http.post($scope.autosaveUrl + '?participantId=' + addId);
                } else if (newValue.length < oldValue.length) {
                    // something was removed
                    var removeId = _.difference(oldValue, newValue);
                    $http.delete($scope.autosaveUrl + '?participantId=' + removeId);
                }
            });
        }]);
})(angular, app);
