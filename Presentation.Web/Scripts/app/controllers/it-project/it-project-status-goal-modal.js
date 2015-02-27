(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.status-goal.modal', {
            url: '/modal/:goalId',
            onEnter: ['$state', '$stateParams', '$modal', 'goalTypes', 'project', 'user',
                function ($state, $stateParams, $modal, goalTypes, project, user) {
                    $modal.open({
                        size: 'lg',
                        templateUrl: 'partials/it-project/modal-goal-edit.html',
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: 'modal fade in',
                        resolve: {
                            goalId: function() {
                                return $stateParams.goalId;
                            },
                            user: function () {
                                return user;
                            },
                            // workaround to get data inherited from parent state
                            goalTypes: function() {
                                return goalTypes;
                            },
                            // workaround to get data inherited from parent state
                            project: function () {
                                return project;
                            },
                            goal: ['$http', function($http) {
                                var id = $stateParams.goalId;
                                if (id) {
                                    return $http.get('api/goal/' + id)
                                        .then(function(result) {
                                            return result.data.response;
                                        });
                                }
                                return null;
                            }]
                        },
                        controller: 'project.statusGoalModalCtrl'
                    }).result.then(function () {
                        // OK
                        // GOTO parent state and reload
                        $state.go('^', null, { reload: true });
                    }, function () {
                        // Cancel
                        // GOTO parent state
                        $state.go('^');
                    });
                }
            ]
        });
    }]);

    app.controller('project.statusGoalModalCtrl',
    ['$scope', '$http', 'autofocus', 'goal', 'notify', 'goalId', 'goalTypes', 'project', 'user',
        function ($scope, $http, autofocus, goal, notify, goalId, goalTypes, project, user) {
            var isNewGoal = goal == null;
            
            // set to empty object if falsy
            $scope.goal = goal ? goal : {};
            $scope.goalTypes = goalTypes;

            autofocus();

            $scope.opened = {};
            $scope.open = function ($event, datepicker) {
                $event.preventDefault();
                $event.stopPropagation();

                $scope.opened[datepicker] = true;
            };

            $scope.dismiss = function () {
                $scope.$dismiss();
            };

            $scope.save = function () {
                var payload = $scope.goal;
                payload.goalStatusId = project.goalStatus.id,
                delete payload.id;
                delete payload.objectOwnerId;
                delete payload.objectOwner;

                var msg = notify.addInfoMessage("Gemmer ændringer...", false);
                $http({
                    method: isNewGoal ? 'POST' : 'PATCH',
                    url: 'api/goal/' + goalId + '?organizationId=' + user.currentOrganizationId,
                    data: payload
                }).success(function () {
                    msg.toSuccessMessage("Ændringerne er gemt!");
                    $scope.$close(true);
                }).error(function () {
                    msg.toErrorMessage("Ændringerne kunne ikke gemmes!");
                });
            };
        }]);
})(angular, app);
