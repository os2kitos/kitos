(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('edit-it-project.org', {
            url: '/org',
            templateUrl: 'partials/it-project/tab-org.html',
            controller: 'project.EditOrgCtrl',
            resolve: {
                isTransversal: ['itProject', function (itProject) {
                    return itProject.isTransversal;
                }],
                selectedOrgUnits: ['itProject', function (itProject) {
                    return _.pluck(itProject.usedByOrgUnits, 'id');
                }],
                orgUnitsTree: ['$http', 'itProject', function ($http, itProject) {
                    return $http.get('api/organizationunit/?organization=' + itProject.organizationId)
                        .then(function (result) {
                            return [result.data.response]; // to array for ngRepeat to work
                        });
                }]
            }
        });
    }]);

    app.controller('project.EditOrgCtrl', ['$scope', '$http', '$stateParams', 'notify', 'isTransversal', 'orgUnitsTree', 'selectedOrgUnits', function ($scope, $http, $stateParams, notify, isTransversal, orgUnitsTree, selectedOrgUnits) {
        $scope.orgUnitsTree = orgUnitsTree;
        $scope.isTransversal = isTransversal;
        var projectId = $stateParams.id;
        $scope.patchUrl = 'api/itproject/' + projectId;

        $scope.save = function (obj) {
            var msg = notify.addInfoMessage("Gemmer... ");
            if (obj.selected) {
                $http.post('api/itproject/' + projectId + '?organizationunit=' + obj.id)
                    .success(function () {
                        msg.toSuccessMessage("Gemt!");
                    })
                    .error(function() {
                        msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                    });
            } else {
                $http.delete('api/itproject/' + projectId + '?organizationunit=' + obj.id)
                    .success(function() {
                        msg.toSuccessMessage("Gemt!");
                    })
                    .error(function() {
                        msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                    });
            }
        };

        $scope.saveState = function() {
            console.log($scope.isTransversal);
            $http
        };

        function searchTree(element, matchingId) {
            if (element.id == matchingId) {
                return element;
            } else if (element.children != null) {
                var result = null;
                for (var i = 0; result == null && i < element.children.length; i++) {
                    result = searchTree(element.children[i], matchingId);
                }
                return result;
            }
            return null;
        }

        _.each(selectedOrgUnits, function (id) {
            var found = searchTree(orgUnitsTree[0], id);
            if (found) {
                found.selected = true;
            }
        });
    }]);
})(angular, app);