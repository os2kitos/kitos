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
    ['$scope', '$http', '$state', 'notify', '$modal', 'project', 'usersWithRoles',
        function ($scope, $http, $state, notify, $modal, project, usersWithRoles) {
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
            
            function addStatus(activity, skipAdding) {
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

                activity.edit = function () {
                    if (activity.isTask) {
                        $state.go('.modal', { type: 'assignment', activityId: activity.id });
                    } else if (activity.isMilestone) {
                        $state.go('.modal', { type: 'milestone', activityId: activity.id });
                    }
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
                addStatus(value);
            });

            $scope.pagination = {
                skip: 0,
                take: 50
            };
            
            $scope.$watchCollection('pagination', loadStatues);

            function loadStatues() {
                var url = 'api/itProjectStatus/' + project.id + '?project=true';

                url += '&skip=' + $scope.pagination.skip;
                url += '&take=' + $scope.pagination.take;

                if ($scope.pagination.orderBy) {
                    url += '&orderBy=' + $scope.pagination.orderBy;
                    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                }

                if ($scope.pagination.search) url += '&q=' + $scope.pagination.search;
                else url += "&q=";
                
                $scope.milestonesActivities = [];
                $http.get(url).success(function (result, status, headers) {
                    var paginationHeader = JSON.parse(headers('X-Pagination'));
                    $scope.totalCount = paginationHeader.TotalCount;

                    _.each(result.response, function (value) {
                        addStatus(value);
                    });

                }).error(function (data, status) {
                    // only display error when an actual error
                    // 404 just says that there are no statues
                    if (status != 404)
                        notify.addErrorMessage("Kunne ikke hente projekter!");
                });
            }
        }
    ]);
})(angular, app);
