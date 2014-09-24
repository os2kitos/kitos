(function (ng, app) {


    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('it-system.edit.kle', {
            url: '/kle',
            templateUrl: 'partials/it-system/tab-edit-kle.html',
            controller: 'system.SystemKleCtrl',
            resolve: {
            }
        });
    }]);

    app.controller('system.SystemKleCtrl', ['$scope', '$http', 'notify', 'itSystem',
        function ($scope, $http, notify, itSystem) {
            var systemId = itSystem.id;
            var baseUrl = 'api/itSystem/' + systemId;

            $scope.pagination = {
                skip: 0,
                take: 50
            };

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

            function loadTasks() {

                var url = baseUrl + "?tasks";

                url += '&onlySelected=' + !$scope.showAllTasks;

                url += '&taskGroup=' + $scope.selectedTaskGroup;
                
                url += '&skip=' + $scope.pagination.skip + '&take=' + $scope.pagination.take;

                if ($scope.pagination.orderBy) {
                    url += '&orderBy=' + $scope.pagination.orderBy;
                    if ($scope.pagination.descending) url += '&descending=' + $scope.pagination.descending;
                }

                $http.get(url).success(function (result, status, headers) {
                    $scope.tasklist = result.response;

                    var paginationHeader = JSON.parse(headers('X-Pagination'));
                    $scope.pagination.count = paginationHeader.TotalCount;
                }).error(function () {
                    notify.addErrorMessage("Kunne ikke hente opgaver!");
                });

            }

            function add(task) {
                return $http.post(baseUrl + '?taskId=' + task.taskRef.id).success(function (result) {
                    task.isSelected = true;
                });
            }

            function remove(task) {
                return $http.delete(baseUrl + '?taskId=' + task.taskRef.id).success(function (result) {
                    task.isSelected = false;
                });
            }

            $scope.save = function (task) {
                var msg = notify.addInfoMessage("Opdaterer ...", false);

                if (!task.isSelected) {
                    add(task).success(function () {
                        msg.toSuccessMessage("Feltet er opdateret!");
                    }).error(function () {
                        msg.toErrorMessage("Fejl!");
                    });
                } else {
                    remove(task).success(function () {
                        msg.toSuccessMessage("Feltet er opdateret!");
                    }).error(function () {
                        msg.toErrorMessage("Fejl!");
                    });
                }
            };

            $scope.selectAllTasks = function () {
                _.each($scope.tasklist, function (task) {
                    if (!task.isSelected) {
                        add(task);
                    }
                });
            };

            $scope.removeAllTasks = function () {
                _.each($scope.tasklist, function (task) {
                    if (task.isSelected) {
                        remove(task);
                    }
                });
            };
        }]);

})(angular, app);