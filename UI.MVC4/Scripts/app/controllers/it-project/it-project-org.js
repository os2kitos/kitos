(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project.edit.org', {
            url: '/org',
            templateUrl: 'partials/it-project/tab-org.html',
            controller: 'project.EditOrgCtrl',
            resolve: {
                // re-resolve data from parent cause changes here wont cascade to it
                project: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("api/itproject/" + $stateParams.id)
                        .then(function (result) {
                            return result.data.response;
                        });
                }],
                isTransversal: ['project', function (project) {
                    return project.isTransversal;
                }],
                selectedOrgUnits: ['project', function (project) {
                    return project.usedByOrgUnits;
                }],
                responsibleOrgUnitId: ['project', function (project) {
                    return project.responsibleOrgUnitId;
                }],
                orgUnitsTree: ['$http', 'project', function ($http, project) {
                    return $http.get('api/organizationunit/?organization=' + project.organizationId)
                        .then(function (result) {
                            return [result.data.response]; // to array for ngRepeat to work
                        });
                }]
            }
        });
    }]);

    app.controller('project.EditOrgCtrl',
        ['$scope', '$http', '$stateParams', 'notify', 'isTransversal', 'orgUnitsTree', 'selectedOrgUnits', 'responsibleOrgUnitId',
            function ($scope, $http, $stateParams, notify, isTransversal, orgUnitsTree, selectedOrgUnits, responsibleOrgUnitId) {
                $scope.orgUnitsTree = orgUnitsTree;
                $scope.isTransversal = isTransversal;
                $scope.selectedOrgUnits = selectedOrgUnits;
                $scope.responsibleOrgUnitId = responsibleOrgUnitId;
                var projectId = $stateParams.id;
                $scope.patchUrl = 'api/itproject/' + projectId;

                $scope.save = function(obj) {
                    var msg = notify.addInfoMessage("Gemmer... ");
                    if (obj.selected) {
                        $http.post('api/itproject/' + projectId + '?organizationunit=' + obj.id)
                            .success(function() {
                                msg.toSuccessMessage("Gemt!");
                                $scope.selectedOrgUnits.push(obj);
                            })
                            .error(function() {
                                msg.toErrorMessage("Fejl! Kunne ikke gemmes!");
                            });
                    } else {
                        $http.delete('api/itproject/' + projectId + '?organizationunit=' + obj.id)
                            .success(function() {
                                msg.toSuccessMessage("Gemt!");

                                var indexOf;
                                var found = _.filter($scope.selectedOrgUnits, function(element, index) {
                                    var equal = element.id == obj.id;
                                    if (equal) indexOf = index;
                                    return equal;
                                });
                                if (found) $scope.selectedOrgUnits.splice(indexOf, 1);
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
                _.each(selectedOrgUnitIds, function(id) {
                    var found = searchTree(orgUnitsTree[0], id);
                    if (found) {
                        found.selected = true;
                    }
                });
            }
        ]
    );
})(angular, app);