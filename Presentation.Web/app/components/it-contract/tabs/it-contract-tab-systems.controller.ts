(function(ng, app) {
    app.config([
        '$stateProvider', function($stateProvider) {
            $stateProvider.state('it-contract.edit.systems', {
                url: '/systems',
                templateUrl: 'app/components/it-contract/tabs/it-contract-tab-systems.view.html',
                controller: 'contract.EditSystemsCtrl',
                resolve: {
                    user: [
                        'userService', function(userService) {
                            return userService.getUser();
                        }
                    ],
                    exhibitedInterfaces: [
                        '$http', 'contract', function($http, contract) {
                            return $http.get('api/ItInterfaceExhibitUsage/?contractId=' + contract.id).then(function(result) {
                                return result.data.response;
                            });
                        }
                    ],
                    usedInterfaces: [
                        '$http', 'contract', function($http, contract) {
                            return $http.get('api/InterfaceUsage/?contractId=' + contract.id).then(function(result) {
                                return result.data.response;
                            });
                        }
                    ]
                }
            });
        }
    ]);

    app.controller('contract.EditSystemsCtrl', [
        '$scope', '$http', '$state', 'notify', 'user', 'contract', 'exhibitedInterfaces', 'usedInterfaces',
        function($scope, $http, $state, notify, user, contract, exhibitedInterfaces, usedInterfaces) {

            $scope.exhibitedInterfaces = exhibitedInterfaces;
            $scope.usedInterfaces = usedInterfaces;

            $scope.deleteExhibit = function(exhibitId, usageId) {
                $http({
                        method: 'PATCH',
                        url: 'api/itInterfaceExhibitUsage/?usageId=' + usageId + '&exhibitId=' + exhibitId,
                        data: {
                            itContractId: null
                        }
                    })
                    .success(function() {
                        notify.addSuccessMessage('Snitfladerelationen er slettet.');
                        reload();
                    })
                    .error(function() {
                        notify.addSuccessMessage('Fejl! Snitfladerelationen kunne ikke slettes.');
                    });
            }

            $scope.deleteUsed = function(usageId, sysId, interfaceId) {
                $http({
                        method: 'PATCH',
                        url: 'api/interfaceUsage/?usageId=' + usageId + '&sysId=' + sysId + '&interfaceId=' + interfaceId + '&organizationId=' + user.currentOrganizationId,
                        data: {
                            itContractId: null
                        }
                    })
                    .success(function() {
                        notify.addSuccessMessage('Snitfladerelationen er slettet.');
                        reload();
                    })
                    .error(function() {
                        notify.addSuccessMessage('Fejl! Snitfladerelationen kunne ikke slettes.');
                    });
            }

            function reload() {
                $state.go('.', null, { reload: true });
            }

            function formatAssociatedSystems(associatedSystemUsages) {

                //helper functions
                function deleteAssociatedSystem(associatedSystem) {
                    return $http.delete('api/itContract/' + contract.id + '?systemUsageId=' + associatedSystem.id + '&organizationId=' + user.currentOrganizationId);
                }

                function postAssociatedSystem(associatedSystem) {
                    return $http.post('api/itContract/' + contract.id + '?systemUsageId=' + associatedSystem.selectedSystem.id + '&organizationId=' + user.currentOrganizationId);
                }

                //for each row of associated system
                _.each(associatedSystemUsages, function(systemUsage: { show; delete; }) {

                    systemUsage.show = true;

                    //delete the row
                    systemUsage.delete = function() {
                        deleteAssociatedSystem(systemUsage).success(function() {
                            notify.addSuccessMessage('Rækken er slettet.');
                            systemUsage.show = false;
                        }).error(function() {
                            notify.addErrorMessage('Kunne ikke slette rækken');
                        });
                    };

                });

                $scope.associatedSystemUsages = associatedSystemUsages;

                $scope.newAssociatedSystemUsage = {
                    save: function() {
                        //post new binding
                        postAssociatedSystem($scope.newAssociatedSystemUsage).success(function(result) {

                            notify.addSuccessMessage('Rækken er tilføjet.');

                            //then reformat and redraw the rows
                            formatAssociatedSystems(result.response);

                        }).error(function(result) {

                            //couldn't add new binding
                            notify.addErrorMessage('Fejl! Kunne ikke tilføje rækken!');

                        });
                    }
                };
            }

            formatAssociatedSystems(contract.associatedSystemUsages);

            $scope.newAssociatedInterfaceSave = function() {
                var url = '';
                if ($scope.newAssociatedInterfaceRelation == 'exhibit')
                    url = 'api/itInterfaceExhibitUsage?usageId=' + $scope.newAssociatedInterfaceSelectedSystemUsage.id + '&exhibitId=' + $scope.newAssociatedInterfaceSelectedInterfaceUsage.id;

                if ($scope.newAssociatedInterfaceRelation == 'using')
                    url = 'api/interfaceUsage?usageId=' + $scope.newAssociatedInterfaceSelectedSystemUsage.id
                        + '&sysId=' + $scope.newAssociatedInterfaceSelectedInterfaceUsage.id.sysId
                        + '&interfaceId=' + $scope.newAssociatedInterfaceSelectedInterfaceUsage.id.intfId
                        + '&organizationId=' + user.currentOrganizationId;

                $http({
                        method: 'PATCH',
                        url: url,
                        data: {
                            itContractId: contract.id
                        }
                    })
                    .success(function() {
                        reload();
                    });
            }

            // select2 options for looking up a system's interfaces
            $scope.itInterfaceUsagesSelectOptions = {
                minimumInputLength: 1,
                initSelection: function(elem, callback) {
                },
                ajax: {
                    data: function(term, page) {
                        return { query: term };
                    },
                    quietMillis: 500,
                    transport: function(queryParams) {
                        var url = '';
                        if ($scope.newAssociatedInterfaceRelation == 'exhibit')
                            url = 'api/exhibit?sysId=' + $scope.newAssociatedInterfaceSelectedSystemUsage.itSystemId + '&orgId=' + user.currentOrganizationId + '&q=' + queryParams.data.query;

                        if ($scope.newAssociatedInterfaceRelation == 'using')
                            url = 'api/itInterfaceUse?sysId=' + $scope.newAssociatedInterfaceSelectedSystemUsage.itSystemId + '&orgId=' + user.currentOrganizationId + '&q=' + queryParams.data.query;

                        var res = $http.get(url).then(queryParams.success);
                        res.abort = function() {
                            return null;
                        };

                        return res;
                    },

                    results: function(data, page) {
                        var results = [];

                        // for each interface usages
                        _.each(data.data.response, function(usage: { itInterfaceId; itSystemId; id; itInterfaceName; }) {
                            results.push({
                                // use the id of the interface usage
                                id: $scope.newAssociatedInterfaceRelation == 'using' ? { intfId: usage.itInterfaceId, sysId: usage.itSystemId } : usage.id,
                                // use the name of the actual interface
                                text: usage.itInterfaceName,
                            });
                        });

                        return { results: results };
                    }
                }
            };

            // select2 options for looking up it system usages
            $scope.itSystemUsagesSelectOptions = {
                minimumInputLength: 1,
                initSelection: function(elem, callback) {
                },
                ajax: {
                    data: function(term, page) {
                        return { query: term };
                    },
                    quietMillis: 500,
                    transport: function(queryParams) {
                        var res = $http.get('api/itSystemUsage?organizationId=' + user.currentOrganizationId + '&q=' + queryParams.data.query).then(queryParams.success);
                        res.abort = function() {
                            return null;
                        };

                        return res;
                    },

                    results: function(data, page) {
                        var results = [];

                        // for each system usages
                        _.each(data.data.response, function(usage: { id; itSystem; }) {
                            results.push({
                                // the id of the system usage id, that is selected
                                id: usage.id,
                                // name of the system is the label for the select2
                                text: usage.itSystem.name,
                                // the if the system id that is selected
                                itSystemId: usage.itSystem.id,
                            });
                        });

                        return { results: results };
                    }
                }
            };
        }
    ]);
})(angular, app);
