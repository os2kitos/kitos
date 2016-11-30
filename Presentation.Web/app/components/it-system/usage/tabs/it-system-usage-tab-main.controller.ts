(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.usage.main', {
            url: '/main',
            templateUrl: 'app/components/it-system/usage/tabs/it-system-usage-tab-main.view.html',
            controller: 'system.EditMain',
            resolve: {
                businessTypes: [
                    '$http', function ($http) {
                        return $http.get("odata/LocalBusinessTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                            .then(function (result) {
                                return result.data.value;
                            });
                    }
                ],
                archiveTypes: [
                    '$http', function ($http) {
                        return $http.get('odata/LocalArchiveTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc')
                            .then(function (result) {
                                return result.data.value;
                            });
                    }
                ],
                sensitiveDataTypes: [
                    '$http', function ($http) {
                        return $http.get('odata/LocalSensitiveDataTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc')
                            .then(function (result) {
                                return result.data.value;
                            });
                    }
                ]
            }
        });
    }]);

    app.controller('system.EditMain', ['$rootScope', '$scope', '$http', '$stateParams', 'notify', 'itSystemUsage',
        'businessTypes', 'archiveTypes', 'sensitiveDataTypes', 'autofocus',
        function ($rootScope, $scope, $http, $stateParams, notify, itSystemUsage, businessTypes, archiveTypes, sensitiveDataTypes, autofocus) {
            $rootScope.page.title = 'IT System - Anvendelse';

            autofocus();
            
            $scope.usageId = $stateParams.id;
            $scope.businessTypes = businessTypes;
            $scope.archiveTypes = archiveTypes;
            $scope.sensitiveDataTypes = sensitiveDataTypes;
            $scope.usage = itSystemUsage;

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

                            console.log(data);

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
