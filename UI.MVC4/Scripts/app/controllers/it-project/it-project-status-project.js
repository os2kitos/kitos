(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('edit-it-project.status-project', {
            url: '/status-project',
            templateUrl: 'partials/it-project/tab-status-project.html',
            controller: 'project.EditStatusProjectCtrl',
            resolve: {
                itProjectRights: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itprojectright/" + $stateParams.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                itProjectRoles: ['$http', function ($http) {
                    return $http.get("api/itprojectrole/")
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('project.EditStatusProjectCtrl',
    ['$scope', '$http', 'notify', 'itProject', 'itProjectRights', 'itProjectRoles',
        function ($scope, $http, notify, itProject, itProjectRights, itProjectRoles) {
            $scope.project = itProject;
            $scope.project.updateUrl = "api/itproject/" + itProject.id;

            //Setup phases
            $scope.project.phases = [itProject.phase1, itProject.phase2, itProject.phase3, itProject.phase4, itProject.phase5];
            var prevPhase = null;
            _.each($scope.project.phases, function (phase) {
                phase.updateUrl = "api/activity/" + phase.id;
                phase.prevPhase = prevPhase;
                prevPhase = phase;
            });
            
            //Returns a phase (activity) given an id
            function findPhase(id) {
                return _.findWhere($scope.project.phases, { id: id });
            }
            
            //All activities - both activities ("opgaver") and milestones
            $scope.milestonesActivities = [];

            //Activities "opgaver"
            _.each(itProject.taskActivities, function(taskActivity) {

                taskActivity.type = "Opgave";
                taskActivity.phase = findPhase(taskActivity.associatedActivityId);

                if (taskActivity.associatedUser) taskActivity.associatedUserRoleNames = getUserRoles(taskActivity.associatedUser.id);

                $scope.milestonesActivities.push(taskActivity);
            });

            function patch(url, field, value) {
                var payload = {};
                payload[field] = value;

                return $http({
                    method: 'PATCH',
                    url: url,
                    data: payload
                });
            }

            $scope.updateSelectedPhase = function (phase) {
                patch($scope.project.updateUrl, "currentPhaseId", phase.id)
                    .success(function (result) {
                        notify.addSuccessMessage("Feltet er opdateret");
                        $scope.project.currentPhaseId = phase.id;
                    })
                    .error(function () {
                        notify.addErrorMessage("Fejl!");
                    });
            };


            $scope.updatePhaseDate = function(phase) {
                //Update start date of the current phase
                patch(phase.updateUrl, "startDate", phase.startDate)
                    .success(function (result) {
                        //Also update end date of the previous phase
                        patch(phase.prevPhase.updateUrl, "endDate", phase.startDate).success(function () {
                            notify.addSuccessMessage("Feltet er opdateret");
                        }).error(function () {
                            notify.addErrorMessage("Fejl!");
                        });

                    }).error(function () {
                        notify.addErrorMessage("Fejl!");
                    });
            };

            $scope.updateStatusDate = function() {
                patch($scope.project.updateUrl, "statusDate", $scope.project.statusDate)
                    .success(function () {
                        notify.addSuccessMessage("Feltet er opdateret");
                    }).error(function () {
                        notify.addErrorMessage("Fejl!");
                    });
            };
            
            function autoSaveTrafficLight(url, field, watchExp) {
                $scope.$watch(watchExp, function(newVal, oldVal) {

                    if (angular.isUndefined(newVal) || newVal == null || newVal == oldVal) return;

                    var msg = notify.addInfoMessage("Gemmer...", false);
                    patch(url, field, newVal).success(function(result) {
                        msg.toSuccessMessage("Feltet er opdateret");
                    }).error(function() {
                        msg.toErrorMessage("Fejl!");
                    });

                });
            }

            autoSaveTrafficLight($scope.project.updateUrl, "statusProject", function() {
                return $scope.project.statusProject;
            });
            
            function getRoleName(roleId) {
                var role = _.findWhere(itProjectRoles, { id: roleId });
                if (role) return role.name;
            }
            
            function getUserRoles(userId) {

                var rights = _.where(itProjectRights, { userId: userId });
                return _.map(rights, function(right) {
                    return getRoleName(right.roleId);
                });
            }

        }]);
})(angular, app);
