(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.status-project.modal', {
            url: '/modal/:type/:activityId',
            onEnter: ['$state', '$stateParams', '$uibModal', 'project', 'usersWithRoles', 'user', "hasWriteAccess",
                function ($state, $stateParams, $modal, project, usersWithRoles, user, hasWriteAccess) {
                    $modal.open({
                        templateUrl: "app/components/it-project/tabs/it-project-tab-status-modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        resolve: {
                            activityId: function () {
                                return $stateParams.activityId;
                            },
                            user: function () {
                                return user;
                            },
                            activityType: function () {
                                return $stateParams.type;
                            },
                            // workaround to get data inherited from parent state
                            project: function () {
                                return project;
                            },
                            // workaround to get data inherited from parent state
                            usersWithRoles: function () {
                                return usersWithRoles;
                            },
                            activity: ["$http", function ($http) {
                                var id = $stateParams.activityId;
                                if (id) {
                                    if ($stateParams.type == "assignment") {
                                        return $http.get("api/assignment/" + id)
                                            .then(function (result) {
                                                return result.data.response;
                                            });
                                    } else if ($stateParams.type == "milestone") {
                                        return $http.get("api/milestone/" + id)
                                            .then(function (result) {
                                                return result.data.response;
                                            });
                                    }
                                }
                                return null;
                            }]
                            ,
                            hasWriteAccess: () => hasWriteAccess
                        },
                        controller: "project.statusModalCtrl"
                    }).result.then(function () {
                        // OK
                        // GOTO parent state and reload
                        $state.go("^", null, { reload: true });
                    }, function () {
                        // Cancel
                        // GOTO parent state
                        $state.go("^");
                    });
                }
            ]
        });
    }]);

    app.controller("project.statusModalCtrl",
        ["$scope", "$http", "autofocus", "project", "usersWithRoles", "activity", "notify", "activityId", "activityType", "hasWriteAccess", "user",
            function ($scope, $http, autofocus, project, usersWithRoles, activity, notify, activityId, activityType, hasWriteAccess, user) {
                var isNewActivity = activity == null;

                $scope.hasWriteAccess = isNewActivity ? true : hasWriteAccess;
                $scope.isAssignment = activityType == "assignment";
                $scope.isMilestone = activityType == "milestone";

                if (activity) {
                    if (activity.startDate) {
                        activity.startDate = moment(activity.startDate, "YYYY-MM-DD").format(Kitos.Constants.DateFormat.DanishDateFormat);
                    }
                    if (activity.endDate) {
                        activity.endDate = moment(activity.endDate, "YYYY-MM-DD").format(Kitos.Constants.DateFormat.DanishDateFormat);
                    }
                    if (activity.date) {
                        activity.date = moment(activity.date, "YYYY-MM-DD").format(Kitos.Constants.DateFormat.DanishDateFormat);
                    }
                }

                // set to empty object if falsy
                $scope.activity = activity ? activity : {};
                $scope.phases = project.phases;
                $scope.usersWithRoles = _.values(usersWithRoles);

                $scope.datepickerOptions = {
                    format: "dd-MM-yyyy",
                    parseFormats: ["yyyy-MM-dd"]
                };

                autofocus();

                $scope.dismiss = function () {
                    $scope.$dismiss();
                };

                $scope.save = function () {
                    var payload = $scope.activity;
                    payload.associatedItProjectId = project.id;

                    var startDate = moment(payload.startDate, Kitos.Constants.DateFormat.DanishDateFormat);
                    if (startDate.isValid()) {
                        payload.startDate = startDate.format("YYYY-MM-DD");
                    } else {
                        payload.startDate = null;
                    }

                    var endDate = moment(payload.endDate, Kitos.Constants.DateFormat.DanishDateFormat);
                    if (endDate.isValid()) {
                        payload.endDate = endDate.format("YYYY-MM-DD");
                    } else {
                        payload.endDate = null;
                    }

                    var date = moment(payload.date, Kitos.Constants.DateFormat.DanishDateFormat);
                    if (date.isValid()) {
                        payload.date = date.format("YYYY-MM-DD");
                    } else {
                        payload.date = null;
                    }

                    delete payload.id;
                    delete payload.objectOwnerId;
                    delete payload.objectOwner;
                    delete payload.associatedUser;

                    var msg = notify.addInfoMessage("Gemmer ændringer...", false);
                    $http({
                        method: isNewActivity ? "POST" : "PATCH",
                        url: `api/${activityType}/${activityId}?organizationId=${user.currentOrganizationId}`,
                        data: payload
                    }).then(function onSuccess(result) {
                        msg.toSuccessMessage("Ændringerne er gemt!");
                        $scope.$close(true);
                    }, function onError(result) {
                        msg.toErrorMessage("Ændringerne kunne ikke gemmes!");
                    });
                };
            }]);
})(angular, app);
