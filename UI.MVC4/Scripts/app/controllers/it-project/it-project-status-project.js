(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.status-project', {
            url: '/status-project',
            templateUrl: 'partials/it-project/tab-status-project.html',
            controller: 'project.EditStatusProjectCtrl',
            resolve: {
                // re-resolve data from parent cause changes here wont cascade to it
                project: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itproject/" + $stateParams.id)
                        .then(function (result) {
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

    app.controller('project.EditStatusProjectCtrl',
    ['$scope', '$http', 'notify', '$modal', 'project', 'usersWithRoles',
        function ($scope, $http, notify, $modal, project, usersWithRoles) {
            $scope.project = project;
            $scope.project.updateUrl = "api/itproject/" + project.id;

            //Setup phases
            $scope.project.phases = [project.phase1, project.phase2, project.phase3, project.phase4, project.phase5];
            var prevPhase = null;
            _.each($scope.project.phases, function (phase) {
                phase.updateUrl = "api/itProjectPhase/" + phase.id;
                phase.prevPhase = prevPhase;
                prevPhase = phase;
            });
            
            //All Assignments - both Assignments ("opgaver") and milestones
            $scope.milestonesActivities = [];
            
            function addMilestoneActivity(activity, skipAdding) {
                activity.show = true;

                if (activity.$type.indexOf('Assignment') > -1 ) {
                    activity.isTask = true;
                    activity.updateUrl = "api/Assignment/" + activity.id;
                } else if (activity.$type.indexOf('Milestone') > -1) {
                    activity.isMilestone = true;
                    activity.updateUrl = "api/Milestone/" + activity.id;
                }

                activity.updatePhase = function() {
                    activity.phase = _.findWhere($scope.project.phases, { id: activity.associatedPhaseId });
                };

                activity.updatePhase();

                activity.updateUser = function() {
                    if (activity.associatedUserId) {
                        activity.associatedUser = _.findWhere(usersWithRoles, { id: activity.associatedUserId });
                    }
                };

                activity.updateUser();

                activity.edit = function() {
                    return editActivity(activity);
                };

                activity.delete = function() {
                    var msg = notify.addInfoMessage("Sletter...");
                    $http.delete(activity.updateUrl).success(function() {
                        activity.show = false;
                        msg.toSuccessMessage("Slettet!");
                    }).error(function() {
                        msg.toErrorMessage("Fejl! Kunne ikke slette!");
                    });
                };

                if (!skipAdding)
                    $scope.milestonesActivities.push(activity);

                return activity;
            }

            _.each(project.itProjectStatuses, function(value) {
                addMilestoneActivity(value);
            });
      
            $scope.addMilestone = function() {
                $http.post("api/Milestone", { associatedItProjectId: project.id }).success(function (result) {
                    var milestone = result.response;

                    addMilestoneActivity(milestone);
                    editActivity(milestone);
                });
            };
            
            $scope.addAssignment = function () {
                $http.post("api/Assignment", { associatedItProjectId: project.id }).success(function (result) {
                    var activity = result.response;

                    addMilestoneActivity(activity);
                    editActivity(activity);
                });
            };

            function editActivity(activity) {
                var modal = $modal.open({
                    templateUrl: 'partials/it-project/modal-milestone-task-edit.html',
                    controller: ['$scope', 'autofocus', '$modalInstance', function ($modalScope, autofocus, $modalInstance) {
                        autofocus();
                        
                        $modalScope.activity = angular.copy(activity);

                        $modalScope.phases = $scope.project.phases;
                        $modalScope.usersWithRoles = _.values(usersWithRoles);
                        
                        $modalScope.opened = {};
                        $modalScope.open = function ($event, datepicker) {
                            $event.preventDefault();
                            $event.stopPropagation();

                            $modalScope.opened[datepicker] = true;
                        };

                        $modalScope.save = function () {
                            var msg = notify.addInfoMessage("Gemmer ændringer...", false);

                            var payload = angular.copy($modalScope.activity);
                            delete payload.id;
                            delete payload.objectOwnerId;
                            delete payload.objectOwner;

                            $http({
                                method: 'PATCH',
                                url: $modalScope.activity.updateUrl,
                                data: payload
                            }).success(function (result) {
                                msg.toSuccessMessage("Ændringerne er gemt!");
                                angular.extend(activity, result.response);

                                addMilestoneActivity(activity, true);
                                $modalInstance.close();

                            }).error(function() {
                                msg.toErrorMessage("Ændringerne kunne ikke gemmes!");
                            });
                        };
                    }]
                });
            }
        }]);
})(angular, app);
