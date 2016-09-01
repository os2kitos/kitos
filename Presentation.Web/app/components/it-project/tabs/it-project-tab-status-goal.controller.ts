(function(ng, app) {
    app.config([
        "$stateProvider", function($stateProvider) {
            $stateProvider.state("it-project.edit.status-goal", {
                url: "/status-goal",
                templateUrl: "app/components/it-project/tabs/it-project-tab-status-goal.view.html",
                controller: "project.EditStatusGoalCtrl",
                resolve: {
                    // re-resolve data from parent cause changes here wont cascade to it
                    project: [
                        "$http", "$stateParams", function($http, $stateParams) {
                            return $http.get("api/itproject/" + $stateParams.id)
                                .then(function(result) {
                                    return result.data.response;
                                });
                        }
                    ],
                    goalTypes: [
                        "$http", function($http) {
                            return $http.get("api/goalType").then(function(result) {
                                return result.data.response;
                            });
                        }
                    ]
                }
            });
        }
    ]);

    app.controller("project.EditStatusGoalCtrl", [
        "$scope", "$http", "notify", "$uibModal", "$state", "project", "goalTypes", "user",
        function ($scope, $http, notify, $modal, $state, project, goalTypes: { name }[], user) {
            $scope.goalStatus = project.goalStatus;
            $scope.goalStatus.updateUrl = "api/goalStatus/" + project.goalStatus.id;

            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            $scope.getGoalTypeName = function(goalTypeId) {
                var type = _.find(goalTypes, { id: goalTypeId });

                return type && type.name;
            };

            $scope.goals = [];

            function addGoal(goal) {
                //add goals means show goal in list
                goal.show = true;

                //see if goal already in list - in that case, just update it
                var prevEntry = _.find($scope.goals, { id: goal.id });
                if (prevEntry) {
                    prevEntry = goal;
                    return;
                }

                //otherwise:

                //easy-access functions
                goal.edit = function() {
                    $state.go(".modal", { goalId: goal.id });
                };

                goal.delete = function() {
                    var msg = notify.addInfoMessage("Sletter... ");
                    $http.delete(goal.updateUrl + "?organizationId=" + user.currentOrganizationId).success(function () {
                        goal.show = false;
                        msg.toSuccessMessage("Slettet!");
                    }).error(function() {
                        msg.toErrorMessage("Fejl! Kunne ikke slette!");
                    });
                };

                goal.updateUrl = "api/goal/" + goal.id;
                $scope.goals.push(goal);
            }

            _.each($scope.goalStatus.goals, addGoal);
        }
    ]);
})(angular, app);
