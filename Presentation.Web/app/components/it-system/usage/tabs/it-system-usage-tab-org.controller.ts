(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage.org', {
            url: '/org',
            templateUrl: 'app/components/it-system/usage/tabs/it-system-usage-tab-org.view.html',
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
                    return $http.get('api/itSystemUsageOrgUnitUsage/' + systemUsageId + '?responsible=true')
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
                    $http.post('api/itSystemUsageOrgUnitUsage/?usageId=' + usageId + '&orgUnitId=' + orgUnitId + '&responsible=true')
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Gemt!");
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                        });
                } else {
                    $http.delete('api/itSystemUsageOrgUnitUsage/?usageId=' + usageId + '&responsible=true')
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Gemt!");
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                        });
                }
            };

            $scope.save = function (obj) {
                var msg = notify.addInfoMessage("Gemmer... ");
                if (obj.selected) {
                    $http.post('api/itSystemUsage/' + usageId + '?organizationunit=' + obj.id + '&organizationId=' + user.currentOrganizationId)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Gemt!");
                            $scope.selectedOrgUnits.push(obj);
                        }, function onError(result) {
                            msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                        });
                } else {
                    $http.delete('api/itSystemUsage/' + usageId + '?organizationunit=' + obj.id + '&organizationId=' + user.currentOrganizationId)
                        .then(function onSuccess(result) {
                            msg.toSuccessMessage("Gemt!");

                            var indexOf;
                            // find the index of the orgunit
                            var found = _.filter($scope.selectedOrgUnits, function (element: { id }, index) {
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
                        }, function onError(result) {
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

            var selectedOrgUnitIds = _.map(selectedOrgUnits, 'id');
            _.each(selectedOrgUnitIds, function (id) {
                var found = searchTree(orgUnitsTree[0], id);
                if (found) {
                    found.selected = true;
                }
            });
        }
    ]);
})(angular, app);
