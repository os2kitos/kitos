(function (ng, app) {
    app.config(['$stateProvider', $stateProvider => {
        $stateProvider.state('it-project.edit.status-project.status-edit-modal', {
            url: '/edit',
            onEnter: ['$state', '$stateParams', '$uibModal', 'project', 'user', 'statusUpdates',
                ($state, $stateParams, $modal, project, user, statusUpdates) => {
                    $modal.open({
                        templateUrl: "app/components/it-project/tabs/it-project-tab-status-project-edit-modal.view.html",
                        windowClass: "modal fade in",
                        resolve: {
                            user: () => user,
                            project: () => project,
                            statusUpdates: () => statusUpdates
                        },
                        controller: "project.statusEditModalCtrl",
                        size: 'lg',
                    }).result.then(() => {
                        // OK
                        // GOTO parent state and reload
                        $state.go("^", null, { reload: true });
                    }, () => {
                        // Cancel
                        // GOTO parent state
                        $state.go("^");
                    });
                }
            ]
        });
    }]);

    app.controller("project.statusEditModalCtrl",
        ["$scope", "$http", "project", "notify", "user", "statusUpdates", "moment",
            ($scope, $http, project, notify, user, statusUpdates, moment) => {

                $scope.statusUpdates = statusUpdates;
                $scope.project = project;
                $scope.moment = moment;

                $scope.datepickerOptions = {
                    format: "dd-MM-yyyy",
                    parseFormats: ["yyyy-MM-dd"]
                };
                $scope.dismiss = () => {
                    $scope.$dismiss();
                };
                $scope.save = (obj) => {
                    var payload = {
                        Created: obj.Created,
                        LastChanged: moment()
                    };
                    var created = moment(payload.Created, "DD-MM-YYYY");
                    if (!created.isValid() || isNaN(created.valueOf()) || created.year() < 1000 || created.year() > 2099) {
                        notify.addErrorMessage("Den indtastede dato er ugyldig.");
                    } else {
                        if (created.isValid()) {
                            payload.Created = created.format("YYYY-MM-DD");
                        } else {
                            payload.Created = null;
                        }
                        var msg = notify.addInfoMessage("Gemmer ændringer...", false);
                        $http({
                            method: "PATCH",
                            url: `odata/ItProjectStatusUpdates(${obj.Id})`,
                            data: payload
                        }).then(function onSuccess(result) {
                            msg.toSuccessMessage("Ændringerne er gemt!");
                            $scope.$close(true);
                        }, function onError(result) {
                            msg.toErrorMessage("Ændringerne kunne ikke gemmes!");
                        });
                    }
                };

            }]);
})(angular, app);
