(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.itsys', {
            url: '/itsys',
            templateUrl: 'partials/it-project/tab-itsys.html',
            controller: 'project.EditItsysCtrl',
            resolve: {
                selectedItSystemIds: ['project', function (itProject) {
                    return _.pluck(itProject.itSystems, 'id');
                }],
                itSystemUsages: ['$http', 'project', function ($http, project) {
                    return $http.get('api/itsystemusage/?organizationId=' + project.organizationId)
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('project.EditItsysCtrl',
    ['$scope', '$http', '$stateParams', 'selectedItSystemIds', 'itSystemUsages',
        function ($scope, $http, $stateParams, selectedItSystemIds, itSystemUsages) {
            $scope.itSystemUsages = itSystemUsages;

            _.each(selectedItSystemIds, function (id) {
                var found = _.find($scope.itSystemUsages, function (usage) {
                    return usage.itSystemId == id;
                });
                if (found) {
                    found.selected = true;
                }
            });

            var projectId = $stateParams.id;
            $scope.save = function(usage) {
                if (usage.selected) {
                    $http.post('api/itproject/' + projectId + '?usageId=' + usage.id);
                } else {
                    $http.delete('api/itproject/' + projectId + '?usageId=' + usage.id);
                }
            };
        }]);
})(angular, app);
