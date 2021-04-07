(function (ng, app) {
    app.config([
        '$stateProvider', $stateProvider => {
            $stateProvider.state('it-system.usage.kle', {
                url: '/kle',
                templateUrl: 'app/components/it-system/usage/tabs/it-system-usage-tab-kle.view.html',
                controller: 'system.EditKle',
                resolve: {
                }
            });
        }
    ]);

    app.controller('system.EditKle', [
        '$scope', '$http', '$state', '$stateParams', 'notify', 'user', 'hasWriteAccess', "itSystemUsage",
        ($scope, $http, $state, $stateParams, notify, user, hasWriteAccess, itSystemUsage) => {
            var usageId = $stateParams.id;
            var baseUrl = 'api/itSystemUsage/' + usageId;

            $scope.system = itSystemUsage.itSystem;
            $scope.pagination = {
                skip: 0,
                take: 50
            };
            $scope.hasWriteAccess = hasWriteAccess;

            $scope.showAllTasks = true;

            $scope.$watch("selectedTaskGroup", function (newVal, oldVal) {
                $scope.pagination.skip = 0;
                loadTasks();
            });
            $scope.$watchCollection('pagination', loadTasks);

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
                url += '&onlySelected=' + !$scope.showAllTasks;
                url += '&taskGroup=' + $scope.selectedTaskGroup;
                url += '&skip=' + $scope.pagination.skip + '&take=' + $scope.pagination.take;

                if ($scope.pagination.orderBy) {
                    url += '&orderBy=' + $scope.pagination.orderBy;
                    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                }

                $http.get(url).then(function onSuccess(result) {
                    $scope.tasklist = result.data.response;

                    var paginationHeader = JSON.parse(result.headers('X-Pagination'));
                    $scope.totalCount = paginationHeader.TotalCount;
                }, function onError(result) {
                    notify.addErrorMessage("Kunne ikke hente opgaver!");
                });
            }

            function add(task) {
                return $http.post(baseUrl + '?taskId=' + task.taskRef.id + '&organizationId=' + user.currentOrganizationId)
                    .then(function onSuccess(result) {
                        task.isSelected = true;
                    });
            }

            function remove(task) {
                return $http.delete(baseUrl + '?taskId=' + task.taskRef.id + '&organizationId=' + user.currentOrganizationId)
                    .then(function onSuccess(result) {
                        task.isSelected = false;
                    });
            }

            $scope.save = function (task) {
                var msg = notify.addInfoMessage("Opdaterer ...", false);

                if (!task.isSelected) {
                    add(task).then(function onSuccess(result) {
                        msg.toSuccessMessage("Feltet er opdateret!");
                    }, function onError(result) {
                        msg.toErrorMessage("Fejl!");
                    });
                } else {
                    remove(task).then(function onSuccess(result) {
                        msg.toSuccessMessage("Feltet er opdateret!");
                    }, function onError(result) {
                        msg.toErrorMessage("Fejl!");
                    });
                }
            };

            $scope.selectAllTasks = function () {
                _.each($scope.tasklist, function (task: { isSelected; isLocked; }) {
                    if (!task.isSelected && !task.isLocked) {
                        add(task);
                    }
                });
            };

            $scope.removeAllTasks = function () {
                _.each($scope.tasklist, function (task: { isSelected; isLocked; }) {
                    if (task.isSelected && !task.isLocked) {
                        remove(task);
                    }
                });
            };

            $scope.selectTaskGroup = function () {
                var url = baseUrl + '?taskId=' + $scope.selectedTaskGroup + '&organizationId=' + user.currentOrganizationId;

                var msg = notify.addInfoMessage("Opretter tilknytning...", false);
                $http.post(url)
                    .then(function onSuccess(result) {
                        msg.toSuccessMessage("Tilknytningen er oprettet");
                        reload();
                    }, function onError(result) {
                        msg.toErrorMessage("Fejl! Kunne ikke oprette tilknytningen!");
                    });
            };

            $scope.removeTaskGroup = function () {
                var url = baseUrl + '?taskId=' + $scope.selectedTaskGroup + '&organizationId=' + user.currentOrganizationId;

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
                $state.go('.', null, { reload: true });
            }
        }
    ]);
})(angular, app);
