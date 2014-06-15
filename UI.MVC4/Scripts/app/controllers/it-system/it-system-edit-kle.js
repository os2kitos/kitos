(function (ng, app) {


    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('it-system.edit.kle', {
            url: '/kle',
            templateUrl: 'partials/it-system/tab-edit-kle.html',
            controller: 'system.SystemKleCtrl',
            resolve: {
                selectedKle: ['itSystem', function(itSystem) {
                    return itSystem.taskRefs;
                }],
                kle: ['$http', 'itSystem', function ($http, itSystem) {
                    return $http.get('api/taskref/?orgId=' + itSystem.organizationId)
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('system.SystemKleCtrl', ['$scope', '$http', 'itSystem', 'selectedKle', 'kle',
        function ($scope, $http, itSystem, selectedKle, kle) {
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

            var updateUrl = 'api/itSystem/' + itSystem.id;
            $scope.save = function (task) {
                if (task.selected) {
                    $http.post(updateUrl + '?taskId=' + task.id);
                } else {
                    $http.delete(updateUrl + '?taskId=' + task.id);
                }
            };
        }]);

})(angular, app);