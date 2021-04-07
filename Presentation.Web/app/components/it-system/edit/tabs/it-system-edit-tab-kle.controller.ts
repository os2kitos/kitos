((ng, app) => {
    app.config([
        "$stateProvider", $stateProvider => {
            $stateProvider.state("it-system.edit.kle", {
                url: "/kle",
                templateUrl: "app/components/it-system/edit/tabs/it-system-edit-tab-kle.view.html",
                controller: "system.SystemKleCtrl",
                resolve: {

                }
            });
        }
    ]);

    app.controller("system.SystemKleCtrl", [
        "$scope", "$http", "$state", "notify", "itSystem", "user", "hasWriteAccess",
        ($scope, $http, $state, notify, itSystem, user, hasWriteAccess) => {

            $scope.system = itSystem;
            var baseUrl = `api/itSystem/${itSystem.id}`;

            $scope.pagination = {
                skip: 0,
                take: 50
            };

            $scope.hasWriteAccess = hasWriteAccess;

            $scope.showAllTasks = true;

            $scope.$watch("selectedTaskGroup", (newVal, oldVal) => {
                $scope.pagination.skip = 0;
                loadTasks();
            });
            $scope.$watchCollection("pagination", loadTasks);

            // change between show all tasks and only show active tasks
            $scope.changeTaskView = () => {
                $scope.showAllTasks = !$scope.showAllTasks;
                $scope.pagination.skip = 0;
                loadTasks();
            };

            // default kle sort order
            $scope.pagination.orderBy = "taskKey";

            function loadTasks() {
                var url = baseUrl + "?tasks=true";
                url += `&onlySelected=${!$scope.showAllTasks}`;
                url += `&taskGroup=${$scope.selectedTaskGroup}`;
                url += `&skip=${$scope.pagination.skip}&take=${$scope.pagination.take}`;

                if ($scope.pagination.orderBy) {
                    url += `&orderBy=${$scope.pagination.orderBy}`;
                    if ($scope.pagination.descending) url += `&descending=${$scope.pagination.descending}`;
                }

                $http.get(url).then(function onSuccess(result) {
                    $scope.tasklist = result.data.response;

                    var paginationHeader = JSON.parse(result.headers("X-Pagination"));
                    $scope.totalCount = paginationHeader.TotalCount;
                }, function onError(result) {
                    notify.addErrorMessage("Kunne ikke hente opgaver!");
                });
            }

            function add(task: any);
            function add(task) {
                return $http.post(baseUrl + "?taskId=" + task.taskRef.id + "&organizationId=" + user.currentOrganizationId)
                    .then(function onSuccess(result) {
                        task.isSelected = true;
                    });
            }

            function remove(task: any);
            function remove(task) {
                return $http.delete(baseUrl + "?taskId=" + task.taskRef.id + "&organizationId=" + user.currentOrganizationId)
                    .then(function onSuccess(result) {
                        task.isSelected = false;
                    });
            }

            $scope.save = task => {
                var msg = notify.addInfoMessage("Opdaterer ...", false);

                if (!task.isSelected) {
                    add(task)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Feltet er opdateret!");
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl!");
                        });
                } else {
                    remove(task)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Feltet er opdateret!");
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl!");
                        });
                }
            };

            $scope.selectAllTasks = () => {
                _.each($scope.tasklist, (task: { isSelected }) => {
                    if (!task.isSelected) {
                        add(task);
                    }
                });
            };

            $scope.removeAllTasks = () => {
                _.each($scope.tasklist, (task: { isSelected }) => {
                    if (task.isSelected) {
                        remove(task);
                    }
                });
            };

            $scope.selectTaskGroup = () => {
                var url = baseUrl + "?taskId=" + ($scope.selectedTaskGroup || "") + "&organizationId=" + user.currentOrganizationId;

                var msg = notify.addInfoMessage("Opretter tilknytning...", false);
                $http.post(url)
                    .then(function onSuccess(result) {
                        msg.toSuccessMessage("Tilknytningen er oprettet");
                        reload();
                    }, function onError(result) {
                        msg.toErrorMessage("Fejl! Kunne ikke oprette tilknytningen!");
                    });
            };

            $scope.removeTaskGroup = () => {
                var url = baseUrl + "?taskId=" + ($scope.selectedTaskGroupc || "") + "&organizationId=" + user.currentOrganizationId;

                var msg = notify.addInfoMessage("Fjerner tilknytning...", false);
                $http.delete(url)
                    .then(function onSuccess(result) {
                        msg.toSuccessMessage("Tilknytningen er fjernet");
                        reload();
                    }, function onError(result) {
                        msg.toErrorMessage("Fejl! Kunne ikke fjerne tilknytningen!");
                    });
            };

            function reload() {
                $state.go(".", null, { reload: true });
            }
        }
    ]);
})(angular, app);
