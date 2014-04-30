(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system-usage.org', {
            url: '/org',
            templateUrl: 'partials/it-system/tab-org.html',
            controller: 'system.EditOrg',
            resolve: {
                selectedOrgUnits: ['itSystemUsage', function (itSystemUsage) {
                    return _.pluck(itSystemUsage.usedBy, 'id');
                }],
                orgUnitsTree: ['itSystemUsage', '$http', function (itSystemUsage, $http) {
                    return $http.get('api/organizationunit/?organization=' + itSystemUsage.organizationId)
                        .then(function(result) {
                            return [result.data.response]; // to array for ngRepeat to work
                        });
                }]
            }
        });
    }]);

    app.controller('system.EditOrg', ['$scope', '$http', '$stateParams', 'selectedOrgUnits', 'orgUnitsTree', function ($scope, $http, $stateParams, selectedOrgUnits, orgUnitsTree) {
        $scope.orgUnitsTree = orgUnitsTree;
        var usageId = $stateParams.id;

        $scope.save = function (obj) {
            if (obj.selected) {
                $http.post('api/itsystemusage/' + usageId + '?organizationunit=' + obj.id)
                    .success(function(result) {
                        $scope.orgUnits.push(result.response);
                    });
            } else {
                $http.delete('api/itsystemusage/' + usageId + '?organizationunit=' + obj.id)
                    .success(function() {
                        var indexOf;
                        var found = _.filter($scope.orgUnits, function(element, index) {
                            var equal = element.id == obj.id;
                            if (equal) indexOf = index;
                            return equal;
                        });
                        if (found) $scope.orgUnits.splice(indexOf, 1);
                    });
            }
        };

        function searchTree(element, matchingId) {
            if (element.id == matchingId) {
                return element;
            } else if (element.children != null) {
                var result = null;
                for (i = 0; result == null && i < element.children.length; i++) {
                    result = searchTree(element.children[i], matchingId);
                }
                return result;
            }
            return null;
        }

        _.each(selectedOrgUnits, function(id) {
            var found = searchTree(orgUnitsTree[0], id);
            if (found) {
                found.selected = true;
            }
        });
    }]);
})(angular, app);