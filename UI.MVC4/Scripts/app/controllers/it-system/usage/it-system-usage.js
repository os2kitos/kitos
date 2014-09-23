(function (ng, app) {
    app.config(['$stateProvider', function($stateProvider) {
        $stateProvider.state('it-system.usage', {
            url: '/usage/{id:[0-9]+}',
            templateUrl: 'partials/it-system/usage-it-system.html',
            controller: 'system.UsageCtrl',
            resolve: {
                businessTypes: [
                    '$http', function($http) {
                        return $http.get('api/businesstype')
                            .then(function(result) {
                                return result.data.response;
                            });
                    }
                ],
                archiveTypes: [
                    '$http', function($http) {
                        return $http.get('api/archivetype')
                            .then(function(result) {
                                return result.data.response;
                            });
                    }
                ],
                sensitiveDataTypes: [
                    '$http', function($http) {
                        return $http.get('api/sensitivedatatype')
                            .then(function(result) {
                                return result.data.response;
                            });
                    }
                ],
                itSystemUsage: [
                    '$http', '$stateParams', function($http, $stateParams) {
                        return $http.get('api/itsystemusage/' + $stateParams.id)
                            .then(function(result) {
                                return result.data.response;
                            });
                    }
                ],
                hasWriteAccess: [
                    '$http', '$stateParams', function($http, $stateParams) {
                        return $http.get('api/itsystemusage/' + $stateParams.id + "?hasWriteAccess")
                            .then(function(result) {
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
            
            $scope.itSytemUsagesSelectOptions = selectLazyLoading('api/itsystemusage', false, ['organizationId=' + itSystemUsage.organizationId]);

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

                            _.each(data.data.response, function (obj) {
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