(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.status-project.status-update-modal', {
            url: '/update',
            onEnter: ['$state', '$stateParams', '$uibModal', 'project', 'user', 'statusUpdates',
                function ($state, $stateParams, $modal, project, user, statusUpdates) {
                    $modal.open({
                        templateUrl: "app/components/it-project/tabs/it-project-tab-status-project-update-modal.view.html",
                        // fade in instead of slide from top, fixes strange cursor placement in IE
                        // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                        windowClass: "modal fade in",
                        resolve: {
                            user: function () {
                                return user;
                            },
                            // workaround to get data inherited from parent state
                            project: function () {
                                return project;
                            },
                            statusUpdates: function () {
                                return statusUpdates;
                            }
                        },
                        controller: "project.statusUpdateModalCtrl"
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

    app.controller("project.statusUpdateModalCtrl",
        ["$scope", "$http", "autofocus", "project", "notify", "user", "statusUpdates",
            function ($scope, $http, autofocus, project, notify, user, statusUpdates) {

                $scope.update = {};

                $scope.datepickerOptions = {
                    format: "dd-MM-yyyy",
                    parseFormats: ["yyyy-MM-dd"]
                };

                autofocus();

                $scope.dismiss = function () {
                    $scope.$dismiss();
                };
                $scope.checkDate = (value) => {
                    var date = moment(value, "DD-MM-YYYY");

                    if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                        notify.addErrorMessage("Den indtastede dato er ugyldig.");
                    }
                }
                $scope.save = function () {
                    var payload = $scope.update;
                    payload.AssociatedItProjectId = project.id;

                    var created = moment(payload.Created, "DD-MM-YYYY");
                    if (created.isValid()) {
                        payload.Created = created.format("YYYY-MM-DD");
                    } else {
                        payload.Created = null;
                    }

                    payload.CombinedStatus = payload.CombinedStatus + "";
                    payload.QualityStatus = payload.QualityStatus + "";
                    payload.TimeStatus = payload.TimeStatus + "";
                    payload.ResourcesStatus = payload.ResourcesStatus + "";

                    /* Kommentar */
                    if (payload.IsCombined == "true") {
                        delete payload.QualityStatus;
                        delete payload.TimeStatus;
                        delete payload.ResourcesStatus;
                    } else {
                        delete payload.CombinedStatus;
                    }

                    var msg = notify.addInfoMessage("Gemmer ændringer...", false);
                    $http({
                        method: "POST",
                        url: `odata/ItProjectStatusUpdates?organizationId=${user.currentOrganizationId}`,
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
