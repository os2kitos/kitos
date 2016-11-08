(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.interface-edit.main', {
            url: '/main',
            templateUrl: 'app/components/it-system/it-interface/it-interface-edit-tab-main.view.html',
            controller: 'system.SystemInterfaceMainCtrl',
            resolve: {
                itInterface: [
                    '$http', '$stateParams', function ($http, $stateParams) {
                        var interfaceId = $stateParams.id;
                        return $http.get('api/itInterface/' + interfaceId)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }
                ],
                hasWriteAccess: [
                    '$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                        var interfaceId = $stateParams.id;
                        return $http.get('api/itInterface/' + interfaceId + '?hasWriteAccess=true&organizationId=' + user.currentOrganizationId)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }
                ],
                user: [
                    'userService', function (userService) {
                        return userService.getUser();
                    }
                ]
            }
        });
    }]);

    app.controller('system.SystemInterfaceMainCtrl',
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
            }
        ]);
})(angular, app);
