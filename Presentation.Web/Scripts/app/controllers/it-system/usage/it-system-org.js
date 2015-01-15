(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage.org', {
            url: '/org',
            templateUrl: 'partials/it-system/tab-org.html',
            controller: 'system.EditOrg',
            resolve: {
                selectedOrgUnits: ['$http', '$stateParams', function ($http, $stateParams) {
                    var systemUsageId = $stateParams.id;
                    return $http.get('api/itSystemUsageOrgUnitUsage/' + systemUsageId)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                responsibleOrgUnitId: ['$http', '$stateParams', function ($http, $stateParams) {
                    var systemUsageId = $stateParams.id;
                    return $http.get('api/itSystemUsageOrgUnitUsage/' + systemUsageId + '?responsible')
                        .then(function (result) {
                            if (result.data.response)
                                return result.data.response.id;
                            return null;
                        }
                    );
                }],
                orgUnitsTree: ['$http', 'itSystemUsage', function ($http, itSystemUsage) {
                    return $http.get('api/organizationunit/?organization=' + itSystemUsage.organizationId)
                        .then(function (result) {
                            return [result.data.response]; // to array for ngRepeat to work
                        });
                }]
            }
        });
    }]);

    app.controller('system.EditOrg', ['$scope', '$http', '$stateParams', 'selectedOrgUnits', 'responsibleOrgUnitId', 'orgUnitsTree', 'notify', 'user',
        function ($scope, $http, $stateParams, selectedOrgUnits, responsibleOrgUnitId, orgUnitsTree, notify, user) {
            $scope.orgUnitsTree = orgUnitsTree;
            $scope.selectedOrgUnits = selectedOrgUnits;
            $scope.responsibleOrgUnitId = responsibleOrgUnitId;
            var usageId = $stateParams.id;

            $scope.saveResponsible = function () {
                var orgUnitId = $scope.responsibleOrgUnitId;
                var msg = notify.addInfoMessage("Gemmer... ");
                if ($scope.responsibleOrgUnitId) {
                    $http.post('api/itSystemUsageOrgUnitUsage/?usageId=' + usageId + '&orgUnitId=' + orgUnitId + '&responsible')
                        .success(function () {
                            msg.toSuccessMessage("Gemt!");
                        })
                        .error(function () {
                            msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                        });
                } else {
                    $http.delete('api/itSystemUsageOrgUnitUsage/?usageId=' + usageId + '&responsible')
                        .success(function () {
                            msg.toSuccessMessage("Gemt!");
                        })
                        .error(function () {
                            msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                        });
                }
            };

            $scope.save = function(obj) {
                var msg = notify.addInfoMessage("Gemmer... ");
                if (obj.selected) {
                    $http.post('api/itsystemusage/' + usageId + '?organizationunit=' + obj.id)
                        .success(function() {
                            msg.toSuccessMessage("Gemt!");
                            $scope.selectedOrgUnits.push(obj);
                        })
                        .error(function() {
                            msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                        });
                } else {
                    $http.delete('api/itsystemusage/' + usageId + '?organizationunit=' + obj.id + '&organizationId=' + user.currentOrganizationId)
                        .success(function() {
                            msg.toSuccessMessage("Gemt!");

                            var indexOf;
                            // find the index of the orgunit 
                            var found = _.filter($scope.selectedOrgUnits, function(element, index) {
                                var equal = element.id == obj.id;
                                // set outer scope indexOf, to be used later
                                if (equal) indexOf = index;
                                return equal;
                            });
                            // remove orgunit from the responsible dropdown
                            if (found) $scope.selectedOrgUnits.splice(indexOf, 1);

                            // if responsible is the orgunit being removed unselect it from the dropdown
                            if (obj.id == $scope.responsibleOrgUnitId)
                                $scope.responsibleOrgUnitId = '';
                        })
                        .error(function() {
                            msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                        });
                }
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

            var selectedOrgUnitIds = _.pluck(selectedOrgUnits, 'id');
            _.each(selectedOrgUnitIds, function (id) {
                var found = searchTree(orgUnitsTree[0], id);
                if (found) {
                    found.selected = true;
                }
            });
        }
    ]);
})(angular, app);
