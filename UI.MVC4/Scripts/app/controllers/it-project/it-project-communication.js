(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.communication', {
            url: '/communication',
            templateUrl: 'partials/it-project/tab-communication.html',
            controller: 'project.EditCommunicationCtrl',
            resolve: {
                comms: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get('api/communication/' + $stateParams.id + '?project=true').then(function (result) {
                        return result.data.response;
                    });
                }],
                //returns a map with those users who have a role in this project.
                //the names of the roles is saved in user.roleNames
                usersWithRoles: ['$http', '$stateParams', function ($http, $stateParams) {

                    //get the rights of the projects
                    return $http.get("api/itprojectrights/" + $stateParams.id)
                        .then(function (rightResult) {
                            var rights = rightResult.data.response;

                            //get the role names
                            return $http.get("api/itprojectrole/")
                                .then(function (roleResult) {
                                    var roles = roleResult.data.response;

                                    //the resulting map
                                    var users = {};
                                    _.each(rights, function (right) {

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

    app.controller('project.EditCommunicationCtrl',
    ['$scope', '$http', '$timeout', '$state', '$stateParams', 'comms', 'usersWithRoles',
        function ($scope, $http, $timeout, $state, $stateParams, comms, usersWithRoles) {
            $scope.comms = comms;
            $scope.usersWithRoles = _.values(usersWithRoles);
            
            $scope.comm = {
                itProjectId: $stateParams.id
            };
            $scope.save = function () {
                $scope.$broadcast('show-errors-check-validity');

                if ($scope.commForm.$invalid) { return; }

                $http.post('api/communication', $scope.comm).finally(reload);
            };
            
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