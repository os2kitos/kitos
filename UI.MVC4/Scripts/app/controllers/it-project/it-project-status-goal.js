(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.status-goal', {
            url: '/status-goal',
            templateUrl: 'partials/it-project/tab-status-goal.html',
            controller: 'project.EditStatusGoalCtrl',
            resolve: {

            }
        });
    }]);

    app.controller('project.EditStatusGoalCtrl',
    ['$scope', '$http', 'notify', '$modal', 'itProject', 
        function ($scope, $http, notify, $modal, itProject) {
            $scope.goalStatus = itProject.goalStatus;
            $scope.goalStatus.updateUrl = "api/goalStatus/" + itProject.goalStatus.id;

            $scope.goals = [];
            function addGoal(goal) {
                
                goal.show = true;

                var prevEntry = _.findWhere($scope.goals, { id: goal.id });
                if (prevEntry) {
                    prevEntry = goal;
                    return;
                }
                goal.edit = function() {
                    editGoal(goal);
                };
                
                goal.delete = function() {

                    var msg = notify.addInfoMessage("Sletter... ");
                    $http.delete(goal.updateUrl).success(function() {

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

            function patch(url, field, value) {
                var payload = {};
                payload[field] = value;

                return $http({
                    method: 'PATCH',
                    url: url,
                    data: payload
                });
            }
            
            $scope.updateStatusDate = function () {
                patch($scope.goalStatus.updateUrl, "statusDate", $scope.project.statusDate)
                    .success(function () {
                        notify.addSuccessMessage("Feltet er opdateret");
                    }).error(function () {
                        notify.addErrorMessage("Fejl!");
                    });
            };

            function autoSaveTrafficLight(url, field, watchExp) {
                $scope.$watch(watchExp, function (newVal, oldVal) {

                    if (angular.isUndefined(newVal) || newVal == null || newVal == oldVal) return;

                    var msg = notify.addInfoMessage("Gemmer...", false);
                    patch(url, field, newVal).success(function (result) {
                        msg.toSuccessMessage("Feltet er opdateret");
                    }).error(function () {
                        msg.toErrorMessage("Fejl!");
                    });

                });
            }

            autoSaveTrafficLight($scope.goalStatus.updateUrl, "status", function () {
                return $scope.goalStatus.status;
            });

            $scope.addGoal = function() {
                $http.post("api/goal", {
                    goalStatusId: itProject.goalStatus.id,
                    goalTypeId: 1
                }).success(function(result) {
                    notify.addSuccessMessage("Nyt mål tilføjet!");

                    addGoal(result.response);

                }).error(function() {
                    notify.addErrorMessage("Kunne ikke oprette nyt mål!");
                });

            };
            


            function editGoal(goal) {
                var modal = $modal.open({
                    templateUrl: 'partials/it-project/modal-goal-edit.html',
                    controller: ['$scope', '$modalInstance', function ($modalScope, $modalInstance) {

                        $modalScope.goal = goal;
                    }]
                });
            }


        }]);
})(angular, app);
