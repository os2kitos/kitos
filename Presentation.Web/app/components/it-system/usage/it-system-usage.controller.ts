(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage', {
            url: '/usage/{id:[0-9]+}',
            templateUrl: 'app/components/it-system/usage/it-system-usage.view.html',
            controller: 'system.UsageCtrl',
            resolve: {
                businessTypes: [
                    '$http', function ($http) {
                        return $http.get("odata/LocalBusinessTypes?$filter=IsLocallyAvailable eq true or IsObligatory eq true")
                            .then(function (result) {
                                return result.data.value;
                            });
                    }
                ],
                archiveTypes: [
                    '$http', function ($http) {
                        return $http.get('odata/LocalArchiveTypes?$filter=IsLocallyAvailable eq true or IsObligatory eq true')
                            .then(function (result) {
                                return result.data.value;
                            });
                    }
                ],
                sensitiveDataTypes: [
                    '$http', function ($http) {
                        return $http.get('odata/LocalSensitiveDataTypes?$filter=IsLocallyAvailable eq true or IsObligatory eq true')
                            .then(function (result) {
                                return result.data.value;
                            });
                    }
                ],
                itSystemUsage: [
                    '$http', '$stateParams', function ($http, $stateParams) {
                        return $http.get('api/itSystemUsage/' + $stateParams.id)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }
                ],
                hasWriteAccess: [
                    '$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                        return $http.get('api/itSystemUsage/' + $stateParams.id + "?hasWriteAccess=true&organizationId=" + user.currentOrganizationId)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }
                ]
            }
        });
    }]);

    app.controller('system.UsageCtrl', ['$rootScope', '$scope', '$http', '$stateParams', 'notify', 'itSystemUsage',
        'businessTypes', 'archiveTypes', 'sensitiveDataTypes', 'hasWriteAccess', 'autofocus',
        function ($rootScope, $scope, $http, $stateParams, notify, itSystemUsage, businessTypes, archiveTypes, sensitiveDataTypes, hasWriteAccess, autofocus) {
            $rootScope.page.title = 'IT System - Anvendelse';

            autofocus();

            $scope.hasWriteAccess = hasWriteAccess;
            $scope.usageId = $stateParams.id;
            $scope.status = [{ id: true, name: 'Aktiv' }, { id: false, name: 'Inaktiv' }];
            $scope.businessTypes = businessTypes;
            $scope.archiveTypes = archiveTypes;
            $scope.sensitiveDataTypes = sensitiveDataTypes;
            $scope.usage = itSystemUsage;

            $scope.allowClearOption = {
                allowClear: true
            };

            if (itSystemUsage.overviewId) {
                $scope.usage.overview = {
                    id: itSystemUsage.overviewId,
                    text: itSystemUsage.overviewItSystemName
                };
            }

            $scope.orgUnits = itSystemUsage.usedBy;

            $scope.itSytemUsagesSelectOptions = selectLazyLoading('api/itSystemUsage', false, ['organizationId=' + itSystemUsage.organizationId]);

            function selectLazyLoading(url, excludeSelf, paramAry) {
                return {
                    minimumInputLength: 1,
                    allowClear: true,
                    placeholder: ' ',
                    initSelection: function (elem, callback) {
                    },
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

                            _.each(data.data.response, function (obj: { id; itSystem; cvr; }) {
                                if (excludeSelf && obj.id == $scope.usageId)
                                    return; // don't add self to result

                                results.push({
                                    id: obj.id,
                                    text: obj.itSystem.name,
                                    cvr: obj.cvr
                                });
                            });

                            return { results: results };
                        }
                    }
                };
            }
        }
    ]);
})(angular, app);
