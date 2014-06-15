(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage.kle', {
            url: '/kle',
            templateUrl: 'partials/it-system/tab-kle.html',
            controller: 'system.EditKle',
            resolve: {
                forcedKle: ['itSystemUsage', function(itSystemUsage) {
                    return itSystemUsage.itSystem.taskRefIds;
                }],
                selectedKle: ['itSystemUsage', function (itSystemUsage) {
                    return itSystemUsage.taskRefs;
                }],
                kle: ['$http', 'itSystemUsage', function ($http, itSystemUsage) {
                    return $http.get('api/taskref/?orgId=' + itSystemUsage.organizationId)
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('system.EditKle', ['$scope', '$http', '$stateParams', 'forcedKle', 'selectedKle', 'kle', function ($scope, $http, $stateParams, forcedKle, selectedKle, kle) {
        $scope.forcedKle = forcedKle;
        $scope.selectedKle = selectedKle;
        $scope.kle = kle;
        $scope.kleFilter = { type: 'KLE-Emne' };
        debugger;

        $scope.cleanKleFilter = function () {
            if ($scope.kleFilter.parentId === null) {
                delete $scope.kleFilter.parentId;
            }
        };

        _.each($scope.selectedKle, function (obj) {
            var found = _.find($scope.kle, function(task) {
                return task.id == obj.id;
            });
            if (found) {
                found.selected = true;
            }
        });
        
        _.each($scope.forcedKle, function (id) {
            var found = _.find($scope.kle, function (task) {
                return task.id == id;
            });
            if (found) {
                found.selected = true;
                found.disabled = true;
            }
        });

        var usageId = $stateParams.id;
        $scope.save = function(task) {
            if (task.selected) {
                $http.post('api/itsystemusage/' + usageId + '?taskId=' + task.id);
            } else {
                $http.delete('api/itsystemusage/' + usageId + '?taskId=' + task.id);
            }
        };
    }]);
})(angular, app);