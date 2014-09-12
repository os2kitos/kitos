(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.status-project.modal', {
            url: '/modal/:type/:activityId',
            onEnter: ['$state', '$stateParams', '$modal', 'project', 'usersWithRoles',
                function ($state, $stateParams, $modal, project, usersWithRoles) {
                    $modal.open({
                        templateUrl: 'partials/it-project/modal-milestone-task-edit.html',
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: 'modal fade in',
                        resolve: {
                            activityId: function() {
                                return $stateParams.activityId;
                            },
                            activityType: function () {
                                return $stateParams.type;
                            },
                            // workaround to get data inherited from parent state
                            project: function () {
                                return project;
                            },
                            // workaround to get data inherited from parent state
                            usersWithRoles: function() {
                                return usersWithRoles;
                            },
                            activity: ['$http', function($http) {
                                var id = $stateParams.activityId;
                                if (id) {
                                    if ($stateParams.type == 'assignment') {
                                        return $http.get('api/assignment/' + id)
                                            .then(function (result) {
                                                return result.data.response;
                                            });
                                    } else if ($stateParams.type == 'milestone') {
                                        return $http.get('api/milestone/' + id)
                                            .then(function(result) {
                                                return result.data.response;
                                            });
                                    }
                                }
                                return null;
                            }]
                        },
                        controller: 'project.statusModalCtrl'
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

    app.controller('project.statusModalCtrl',
    ['$scope', '$http', 'autofocus', 'project', 'usersWithRoles', 'activity', 'notify', 'activityId', 'activityType',
        function ($scope, $http, autofocus, project, usersWithRoles, activity, notify, activityId, activityType) {
            var isNewActivity = activity == null;
            
            $scope.isAssignment = activityType == 'assignment';
            $scope.isMilestone = activityType == 'milestone';
            // set to empty object if falsy
            $scope.activity = activity ? activity : {};
            $scope.phases = project.phases;
            $scope.usersWithRoles = _.values(usersWithRoles);

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
                var payload = $scope.activity;
                payload.associatedItProjectId = project.id;
                delete payload.id;
                delete payload.objectOwnerId;
                delete payload.objectOwner;

                var msg = notify.addInfoMessage("Gemmer ændringer...", false);
                $http({
                    method: isNewActivity ? 'POST' : 'PATCH',
                    url: 'api/' + activityType + '/' + activityId,
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
