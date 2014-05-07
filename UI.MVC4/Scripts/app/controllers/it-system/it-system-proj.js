(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system-usage.proj', {
            url: '/proj',
            templateUrl: 'partials/it-system/tab-proj.html',
            controller: 'system.EditProjCtrl',
            resolve: {
                selectedItProjects: ['$http', '$stateParams', 'itSystemUsage', function ($http, $stateParams, itSystemUsage) {
                    return $http.get('api/itproject/?orgId=' + itSystemUsage.organizationId + '&usageId=' + $stateParams.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                itProjects: ['$http', 'itSystemUsage', function ($http, itSystemUsage) {
                    return $http.get('api/itproject/?orgId=' + itSystemUsage.organizationId + '&itProjects=true')
                        .then(function (result) {
                            return result.data.response;
                        });
                }]
            }
        });
    }]);

    app.controller('system.EditProjCtrl', ['$scope', '$http', '$stateParams', 'selectedItProjects', 'itProjects', function ($scope, $http, $stateParams, selectedItProjects, itProjects) {
        $scope.itProjects = itProjects;

        _.each(selectedItProjects, function (obj) {
            var found = _.find($scope.itProjects, function (project) {
                return project.id == obj.id;
            });
            if (found) {
                found.selected = true;
            }
        });

        var usageId = $stateParams.id;
        $scope.save = function (itProject) {
            if (itProject.selected) {
                $http.post('api/itproject/' + itProject.id + '?usageId=' + usageId);
            } else {
                $http.delete('api/itproject/' + itProject.id + '?usageId=' + usageId);
            }
        };
    }]);
})(angular, app);