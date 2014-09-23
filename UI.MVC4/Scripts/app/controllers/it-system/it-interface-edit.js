(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.interface-edit', {
            url: '/edit/{id:[0-9]+}',
            templateUrl: 'partials/it-system/edit-it-interface.html',
            controller: 'system.interfaceEditCtrl',
            resolve: {
                itInterface: [
                    '$http', '$stateParams', function($http, $stateParams) {
                        var interfaceId = $stateParams.id;
                        return $http.get('api/itInterface/' + interfaceId)
                            .then(function(result) {
                                return result.data.response;
                            });
                    }
                ],
                hasWriteAccess: [
                    '$http', '$stateParams', function($http, $stateParams) {
                        var interfaceId = $stateParams.id;
                        return $http.get('api/itInterface/' + interfaceId + '?hasWriteAccess')
                            .then(function(result) {
                                return result.data.response;
                            });
                    }
                ],
                user: [
                    'userService', function(userService) {
                        return userService.getUser();
                    }
                ]
            }
        });
    }]);

    app.controller('system.interfaceEditCtrl',
    [
        '$rootScope', '$scope', '$http', '$state', 'notify', 'itInterface', 'hasWriteAccess', 'autofocus', 'user',
        function ($rootScope, $scope, $http, $state, notify, itInterface, hasWriteAccess, autofocus, user) {
            $rootScope.page.title = 'Snitflade - Rediger';
            autofocus();

            itInterface.belongsTo = (!itInterface.belongsToId) ? null : { id: itInterface.belongsToId, text: itInterface.belongsToName };
            itInterface.updateUrl = 'api/itInterface/' + itInterface.id;
            $scope.interface = itInterface;
            $scope.hasWriteAccess = hasWriteAccess;
            $scope.select2AllowClearOpt = {
                allowClear: true
            };
            
            $scope.organizationSelectOptions = selectLazyLoading('api/organization', true, ['orgId=' + user.currentOrganizationId]);

            function selectLazyLoading(url, allowClear, paramAry) {
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

                            _.each(data.data.response, function (obj) {
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
        }
    ]);
})(angular, app);
