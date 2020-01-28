(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.interface-edit.main', {
            url: '/main',
            templateUrl: 'app/components/it-system/it-interface/it-interface-edit-tab-main.view.html',
            controller: 'system.SystemInterfaceMainCtrl',
            resolve: {
                interfaces: [
                    '$http', function ($http) {
                        return $http.get('odata/LocalInterfaceTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc').then(function (result) {
                            return result.data.value;
                        });
                    }
                ],
                dataTypes: [
                    '$http', function ($http) {
                        return $http.get('odata/LocalDataTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc').then(function (result) {
                            return result.data.value;
                        });
                    }
                ],
                dataRows: [
                    '$http', 'itInterface', function ($http, itInterface) {
                        return $http.get('api/datarow/?interfaceid=' + itInterface.id)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }
                ]
            }
        });
    }]);

    app.controller('system.SystemInterfaceMainCtrl',
        [
            '$rootScope', '$scope', '$http', '$state', 'notify', 'itInterface', 'autofocus', 'user', 'hasWriteAccess', 'interfaces', 'dataTypes','dataRows',
            function ($rootScope, $scope, $http, $state, notify, itInterface, autofocus, user, hasWriteAccess, interfaces, dataTypes, dataRows) {
                $rootScope.page.title = 'Snitflade - Rediger';

                $scope.hasWriteAccess = hasWriteAccess;
                $scope.interfaces = interfaces;
                $scope.dataTypes = dataTypes;

                autofocus();
                $scope.hasWriteAccess = hasWriteAccess;

                itInterface.belongsTo = (!itInterface.belongsToId) ? null : { id: itInterface.belongsToId, text: itInterface.belongsToName };

                let isDisabled = (itInterface.exhibitedByItSystemDisabled) ? " (Slettes)" : "";
                $scope.exposedByObj = !itInterface.exhibitedByItSystemId ? null : { id: itInterface.exhibitedByItSystemId, text: itInterface.exhibitedByItSystemName + isDisabled };

                itInterface.updateUrl = 'api/itInterface/' + itInterface.id;
                $scope.system = itInterface;

                $scope.interface = itInterface;
                $scope.select2AllowClearOpt = {
                    allowClear: true
                };

                $scope.dataRows = [];
                _.each(dataRows, pushDataRow);

                function pushDataRow(dataRow) {
                    dataRow.show = true;
                    dataRow.updateUrl = 'api/dataRow/' + dataRow.id;
                    dataRow.delete = function () {
                        var msg = notify.addInfoMessage('Fjerner rækken...', false);
                        $http.delete(dataRow.updateUrl + '?organizationId=' + user.currentOrganizationId).success(function (result) {
                            dataRow.show = false;
                            msg.toSuccessMessage('Rækken er fjernet!');
                        }).error(function () {
                            msg.toErrorMessage('Fejl! Kunne ikke fjerne rækken!');
                        });
                    };

                    $scope.dataRows.push(dataRow);
                }

                $scope.newDataRow = function () {

                    var payload = { itInterfaceId: itInterface.id };

                    var msg = notify.addInfoMessage('Tilføjer række...', false);
                    $http.post('api/dataRow', payload).success(function (result) {
                        pushDataRow(result.response);
                        msg.toSuccessMessage('Rækken er tilføjet!');
                    }).error(function () {
                        msg.toErrorMessage('Fejl! Kunne ikke tilføje rækken!');
                    });
                };

                $scope.itSystemsSelectOptions = selectLazyLoadingForSystem('api/itsystem?', ['organizationId=' + user.currentOrganizationId]);


                $scope.organizationSelectOptions = selectLazyLoadingForOrg('api/organization', true, ['orgId=' + user.currentOrganizationId]);

                function selectLazyLoadingForOrg(url, allowClear, paramAry) {
                    return {
                        minimumInputLength: 1,
                        initSelection: function (elem, callback) {
                        },
                        allowClear: allowClear,
                        ajax: {
                            data: function (term, page) {
                                return { query: term };
                            },
                            quietMillis: 500,
                            transport: function (queryParams) {
                                var extraParams = paramAry ? '&' + paramAry.join('&') : '';
                                var res = $http.get(url + '?q=' + queryParams.data.query + extraParams).then(queryParams.success);
                                res.abort = function () {
                                    return null;
                                };

                                return res;
                            },

                            results: function (data, page) {
                                var results = [];

                                _.each(data.data.response, function (obj: { id; name; }) {
                                    results.push({
                                        id: obj.id,
                                        text: obj.name
                                    });
                                });

                                return { results: results };
                            }
                        }
                    };
                }

                function selectLazyLoadingForSystem(url, paramAry) {
                    return {
                        allowClear: true,
                        minimumInputLength: 1,
                        initSelection: function (elem, callback) {
                        },
                        ajax: {
                            data: function (term, page) {
                                return { query: term };
                            },
                            quietMillis: 500,
                            transport: function (queryParams) {
                                var extraParams = paramAry ? '&' + paramAry.join('&') : '';
                                var res = $http.get(url + 'q=' + queryParams.data.query + extraParams).then(queryParams.success);
                                res.abort = function () {
                                    return null;
                                };

                                return res;
                            },

                            results: function (data, page) {
                                var results = [];
                                _.each(data.data.response, function (obj: { id; name; disabled; }) {
                                    if (!obj.disabled) {
                                        results.push({
                                            id: obj.id,
                                            text: obj.name
                                        });
                                    }
                                });

                                return { results: results };
                            }
                        }
                    };
                }

                function reload() {
                    $state.go('.', null, { reload: true });
                }

                $scope.save = function () {
                    // select2 is calling save twice,
                    // checking for object will discard the incorrect call
                    if (typeof $scope.exposedByObj !== 'object')
                        return;

                    // check if this interface is exhibited by any system that is in use (itsystemusage)
                    if (itInterface.isUsed) {
                        // clearing or changing the value must result in a dialog prompt
                        if (!$scope.exposedByObj) {
                            if (!confirm('Der er IT Systemer, som er i Lokal anvendelse som har denne snitfladerelation tilknyttet. Er du sikker på at du vil fjerne relationen?')) {
                                $scope.exposedByObj = { id: itInterface.exhibitedByItSystemId, text: itInterface.exhibitedByItSystemName };
                                return;
                            }
                        }
                        // TODO need previous value to prompt when value is changed (not cleared)
                    }

                    var msg = notify.addInfoMessage("Gemmer...", false);
                    if ($scope.exposedByObj) {
                        if (itInterface.exhibitedByItSystemId) {
                            // PATCH
                            var patchPayload = {
                                itSystemId: $scope.exposedByObj.id
                            };
                            var url = 'api/exhibit/' + itInterface.id + '?organizationId=' + user.currentOrganizationId;
                            $http({ method: 'PATCH', url: url, data: patchPayload })
                                .success(function () {
                                    msg.toSuccessMessage("Feltet er opdateret.");
                                    reload();
                                })
                                .error(function () {
                                    msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                                });
                        } else {
                            // POST
                            var postPayload = {
                                id: itInterface.id,
                                itSystemId: $scope.exposedByObj.id
                            };
                            $http.post('api/exhibit', postPayload).success(function () {
                                msg.toSuccessMessage("Feltet er opdateret.");
                                reload();
                            }).error(function () {
                                msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                            });
                        }
                    } else {
                        // DELETE
                        $http.delete('api/exhibit/' + itInterface.id + '?organizationId=' + user.currentOrganizationId).success(function () {
                            msg.toSuccessMessage("Feltet er opdateret.");
                            reload();
                        }).error(function () {
                            msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                        });
                    }
                }
            }
        ]);
})(angular, app);
