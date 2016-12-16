(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system.interface-edit.main', {
            url: '/main',
            templateUrl: 'app/components/it-system/it-interface/it-interface-edit-tab-main.view.html',
            controller: 'system.SystemInterfaceMainCtrl'
        });
    }]);

    app.controller('system.SystemInterfaceMainCtrl',
        [
            '$rootScope', '$scope', '$http', '$state', 'notify', 'itInterface', 'autofocus', 'user', 'hasWriteAccess',
            function ($rootScope, $scope, $http, $state, notify, itInterface, autofocus, user, hasWriteAccess) {
                $rootScope.page.title = 'Snitflade - Rediger';
                autofocus();
                $scope.hasWriteAccess = hasWriteAccess;

                itInterface.belongsTo = (!itInterface.belongsToId) ? null : { id: itInterface.belongsToId, text: itInterface.belongsToName };
                itInterface.updateUrl = 'api/itInterface/' + itInterface.id;
                //itInterface.updateUrl = "odata/itInterfaces(" + itInterface.id + ")";
                $scope.interface = itInterface;
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
