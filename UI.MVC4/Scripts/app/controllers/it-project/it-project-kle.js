(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.kle', {
            url: '/kle',
            templateUrl: 'partials/it-project/tab-kle.html',
            controller: 'project.EditKleCtrl',
            resolve: {
                // re-resolve data from parent cause changes here wont cascade to it
                project: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itproject/" + $stateParams.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }]

            }
        });
    }]);

    app.controller('project.EditKleCtrl',
    ['$scope', '$http', '$stateParams', 'notify',
        function ($scope, $http, $stateParams, notify) {
            var projectId = $stateParams.id;
            var baseUrl = 'api/itProject/' + projectId;
            
            $scope.$watch("selectedTaskGroup", function (newVal, oldVal) {
                clearTasksPagination();
                loadTasks();
            });

            //change between show all tasks and only show active tasks
            $scope.changeTaskView = function () {
                $scope.showAllTasks = !$scope.showAllTasks;

                clearTasksPagination();
                loadTasks();
            };

            var skipTasks = 0;
            var takeTasks = 20;
            function clearTasksPagination() {
                skipTasks = 0;
                }

            $scope.loadLessTasks = function () {
                skipTasks -= takeTasks;
                if (skipTasks < 0) skipTasks = 0;

                loadTasks();
            };

            $scope.loadMoreTasks = function () {
                skipTasks += takeTasks;

                loadTasks();
            };

            function calculatePaginationButtons(headers) {
                $scope.lessTasks = (skipTasks != 0);

                var paginationHeader = JSON.parse(headers('X-Pagination'));
                var count = paginationHeader.TotalCount;
                $scope.moreTasks = (skipTasks + takeTasks) < count;
            }

            function loadTasks() {

                var url = baseUrl + "?tasks";

                url += '&onlySelected=' + !$scope.showAllTasks;

                url += '&taskGroup=' + $scope.selectedTaskGroup;

                url += '&skip=' + skipTasks + '&take=' + takeTasks;
                
                $http.get(url).success(function (result, status, headers) {
                    $scope.tasklist = result.response;
                    calculatePaginationButtons(headers);
                }).error(function () {
                    notify.addErrorMessage("Kunne ikke hente opgaver!");
                });

                }
            
            function add(task) {
                return $http.post(baseUrl + '?taskId=' + task.taskRef.id).success(function(result) {
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
                    add(task).success(function() {
                        msg.toSuccessMessage("Feltet er opdateret!");
                    }).error(function() {
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
