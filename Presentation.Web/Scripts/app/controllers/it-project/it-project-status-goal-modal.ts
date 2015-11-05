﻿(function (ng, app) {
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
    ['$scope', '$http', 'autofocus', 'goal', 'notify', 'goalId', 'goalTypes', 'project', 'user', 'moment',
        function ($scope, $http, autofocus, goal, notify, goalId, goalTypes, project, user, moment) {
            var isNewGoal = goal == null;

            if (goal) {
                if (goal.subGoalDate1) {
                    goal.subGoalDate1 = moment(goal.subGoalDate1, "YYYY-MM-DD").format("DD-MM-YYYY");
                }
                if (goal.subGoalDate2) {
                    goal.subGoalDate2 = moment(goal.subGoalDate2, "YYYY-MM-DD").format("DD-MM-YYYY");
                }
                if (goal.subGoalDate3) {
                    goal.subGoalDate3 = moment(goal.subGoalDate3, "YYYY-MM-DD").format("DD-MM-YYYY");
                }
            }

            // set to empty object if falsy
            $scope.goal = goal ? goal : {};
            $scope.goalTypes = goalTypes;

            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            autofocus();

            $scope.dismiss = function () {
                $scope.$dismiss();
            };

            $scope.save = function () {
                var payload = $scope.goal;
                payload.goalStatusId = project.goalStatus.id;

                var subGoalDate1 = moment(payload.subGoalDate1, "DD-MM-YYYY");
                if (subGoalDate1.isValid()) {
                    payload.subGoalDate1 = subGoalDate1.format("YYYY-MM-DD");
                } else {
                    payload.subGoalDate1 = null;
                }

                var subGoalDate2 = moment(payload.subGoalDate2, "DD-MM-YYYY");
                if (subGoalDate2.isValid()) {
                    payload.subGoalDate2 = subGoalDate2.format("YYYY-MM-DD");
                } else {
                    payload.subGoalDate2 = null;
                }

                var subGoalDate3 = moment(payload.subGoalDate3, "DD-MM-YYYY");
                if (subGoalDate3.isValid()) {
                    payload.subGoalDate3 = subGoalDate3.format("YYYY-MM-DD");
                } else {
                    payload.subGoalDate3 = null;
                }

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
