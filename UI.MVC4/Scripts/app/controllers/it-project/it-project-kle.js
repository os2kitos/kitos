(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('edit-it-project.kle', {
            url: '/kle',
            templateUrl: 'partials/it-project/tab-kle.html',
            controller: 'project.EditKleCtrl',
            resolve: {
                selectedKle: ['itProject', function (itProject) {
                    return itProject.taskRefs;
                }],
                kle: ['$http', 'itProject', function ($http, itProject) {
                    return $http.get('api/taskref/?orgId=' + itProject.organizationId)
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('project.EditKleCtrl',
    ['$scope', '$http', '$stateParams', 'selectedKle', 'kle',
        function ($scope, $http, $stateParams, selectedKle, kle) {
            $scope.selectedKle = selectedKle;
            $scope.kle = kle;
            $scope.kleFilter = { type: 'KLE-Emne' };
            
            $scope.cleanKleFilter = function () {
                if ($scope.kleFilter.parentId === null) {
                    delete $scope.kleFilter.parentId;
                }
            };

            _.each($scope.selectedKle, function (obj) {
                var found = _.find($scope.kle, function (task) {
                    return task.id == obj.id;
                });
                if (found) {
                    found.selected = true;
                }
            });
            
            var projectId = $stateParams.id;
            $scope.save = function (task) {
                if (task.selected) {
                    $http.post('api/itproject/' + projectId + '?taskId=' + task.id);
                } else {
                    $http.delete('api/itproject/' + projectId + '?taskId=' + task.id);
                }
            };
        }]);
})(angular, app);
