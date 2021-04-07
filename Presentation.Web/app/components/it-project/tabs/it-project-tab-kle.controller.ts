(function (ng, app) {
    app.config([
        "$stateProvider", function ($stateProvider) {
            $stateProvider.state("it-project.edit.kle", {
                url: "/kle",
                templateUrl: "app/components/it-project/tabs/it-project-tab-kle.view.html",
                controller: "project.EditKleCtrl",
                resolve: {
                    // re-resolve data from parent cause changes here wont cascade to it
                    project: [
                        "$http", "$stateParams", function ($http, $stateParams) {
                            return $http.get("api/itproject/" + $stateParams.id)
                                .then(function (result) {
                                    return result.data.response;
                                });
                        }
                    ]
                }
            });
        }
    ]);

    app.controller("project.EditKleCtrl",
        [
            "$scope", "$http", "$state", "$stateParams", "notify", "user",
            function ($scope, $http, $state, $stateParams, notify, user) {
                var projectId = $stateParams.id;
                var baseUrl = "api/itProject/" + projectId;

                $scope.pagination = {
                    skip: 0,
                    take: 50
                };

                $scope.showAllTasks = true;

                $scope.$watch("selectedTaskGroup", function (newVal, oldVal) {
                    $scope.pagination.skip = 0;
                    loadTasks();
                });
                //Ændrede metoden til $watchCollection fra $FwCollection
                $scope.$watchCollection("pagination", loadTasks);

                //change between show all tasks and only show active tasks
                $scope.changeTaskView = function () {
                    $scope.showAllTasks = !$scope.showAllTasks;
                    $scope.pagination.skip = 0;
                    loadTasks();
                };

                // default kle sort order
                $scope.pagination.orderBy = "taskKey";

                function loadTasks() {
                    var url = baseUrl + "?tasks=true";
                    url += "&onlySelected=" + !$scope.showAllTasks;
                    url += "&taskGroup=" + $scope.selectedTaskGroup;
                    url += "&skip=" + $scope.pagination.skip + "&take=" + $scope.pagination.take;

                    if ($scope.pagination.orderBy) {
                        url += "&orderBy=" + $scope.pagination.orderBy;
                        if ($scope.pagination.descending) url += "&descending=" + $scope.pagination.descending;
                    }

                    $http.get(url)
                        .then(function onSuccess(result) {
                            $scope.tasklist = result.response;

                            var paginationHeader = JSON.parse(result.headers("X-Pagination"));
                            $scope.totalCount = paginationHeader.TotalCount;
                        }, function onError(result) {
                            notify.addErrorMessage("Kunne ikke hente opgaver!");
                        });
                }

                function add(task) {
                    return $http.post(baseUrl + "?taskId=" + task.taskRef.id + "&organizationId=" + user.currentOrganizationId, { ignoreLoadingBar: true })
                        .then(result => {
                            task.isSelected = true;
                        });
                }

                function remove(task) {
                    return $http.delete(baseUrl + "?taskId=" + task.taskRef.id + "&organizationId=" + user.currentOrganizationId, { ignoreLoadingBar: true })
                        .then(result => {
                            task.isSelected = false;
                        });
                }

                $scope.save = function (task) {
                    var msg = notify.addInfoMessage("Opdaterer ...", false);

                    if (!task.isSelected) {
                        add(task)
                            .then(() => {
                                msg.toSuccessMessage("Feltet er opdateret!");
                            }, () => {
                                msg.toErrorMessage("Fejl!");
                            });
                    } else {
                        remove(task)
                            .then(() => {
                                msg.toSuccessMessage("Feltet er opdateret!");
                            }, () => {
                                msg.toErrorMessage("Fejl!");
                            });
                    }
                };

                $scope.selectAllTasks = function () {
                    _.each($scope.tasklist, function (task: { isSelected }) {
                        if (!task.isSelected) {
                            add(task);
                        }
                    });
                };

                $scope.removeAllTasks = function () {
                    _.each($scope.tasklist, function (task: { isSelected }) {
                        if (task.isSelected) {
                            remove(task);
                        }
                    });
                };

                $scope.selectTaskGroup = function () {
                    var url = baseUrl + "?taskId=" + $scope.selectedTaskGroup + "&organizationId=" + user.currentOrganizationId;

                    var msg = notify.addInfoMessage("Opretter tilknytning...", false);
                    $http.post(url)
                        .then(() => {
                            msg.toSuccessMessage("Tilknytningen er oprettet");
                            reload();
                        }, () => {
                            msg.toErrorMessage("Fejl! Kunne ikke oprette tilknytningen!");
                        });
                };

                $scope.removeTaskGroup = function () {
                    var url = baseUrl + "?taskId=" + $scope.selectedTaskGroup + "&organizationId=" + user.currentOrganizationId;

                    var msg = notify.addInfoMessage("Fjerner tilknytning...", false);
                    $http.delete(url)
                        .then(() => {
                            msg.toSuccessMessage("Tilknytningen er fjernet");
                            reload();
                        }, () => {
                            msg.toErrorMessage("Fejl! Kunne ikke fjerne tilknytningen!");
                        });
                };

                function reload() {
                    $state.go(".", null, { reload: true });
                }
            }
        ]);
})(angular, app);
