(function (ng, app) {
    app.config([
        '$stateProvider', function ($stateProvider) {
            $stateProvider.state('it-system.edit.main', {
                url: '/main',
                templateUrl: 'app/components/it-system/edit/tabs/it-system-edit-tab-main.view.html',
                controller: 'system.SystemMainCtrl',
                resolve: {
                    businessTypes: [
                        '$http', function ($http) {
                            return $http.get("odata/LocalBusinessTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc");
                        }
                    ],
                    appTypeOptions: [
                        '$http', function ($http) {
                            return $http.get("odata/LocalItSystemTypes?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc").then(function (result) {
                                return result.data.value;
                            });
                        }
                    ], itSystem: ['$http', '$stateParams', function ($http, $stateParams) {
                        return $http.get("api/itsystem/" + $stateParams.id)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }]
                }
            });
        }
    ]);

    app.controller('system.SystemMainCtrl',
        [
            '$rootScope', '$scope', '$http', '$state', 'notify', 'itSystem', 'businessTypes', 'user', 'autofocus', 'appTypeOptions', 'hasWriteAccess',
            function ($rootScope, $scope, $http, $state, notify, itSystem, businessTypes, user, autofocus, appTypeOptions, hasWriteAccess) {
                $rootScope.page.title = 'IT System - Rediger system';
                autofocus();

                itSystem.updateUrl = 'api/itsystem/' + itSystem.id;
                itSystem.belongsTo = (!itSystem.belongsToId) ? null : { id: itSystem.belongsToId, text: itSystem.belongsToName };
                itSystem.parent = (!itSystem.parentId) ? null : { id: itSystem.parentId, text: itSystem.parentName };

                $scope.select2AllowClearOpt = {
                    allowClear: true
                };

                $scope.appTypeOptions = appTypeOptions;
                $scope.system = itSystem;
                $scope.businessTypes = businessTypes.data.value;
                $scope.itSystemsSelectOptions = selectLazyLoading('api/itsystem', true, ['excludeId=' + itSystem.id, 'orgId=' + user.currentOrganizationId]);
                $scope.organizationSelectOptions = selectLazyLoading('api/organization', true, ['orgId=' + user.currentOrganizationId]);
            
                $scope.hasWriteAccess = hasWriteAccess;


                $scope.submitDataLevel = function () {
                    var data = {
                        DataLevel: $scope.system.dataLevel
                    };
                    $http.patch("api/itsystem/" + itSystem.id + "?organizationId=" + itSystem.organizationId, data).success(function (result) {
                        notify.addSuccessMessage("Feltet er opdateret.");

                    }).error(function (result) {
                        notify.addErrorMessage('Fejl!');
                    });
                };


                function selectLazyLoading(url, allowClear, paramAry) {
                    return {
                        minimumInputLength: 1,
                        initSelection: (elem, callback) => {
                        },
                        allowClear: allowClear,
                        ajax: {
                            data: (term, page) => ({ query: term }),
                            quietMillis: 500,
                            transport: (queryParams) => {
                                var extraParams = paramAry ? '&' + paramAry.join('&') : '';
                                var res = $http.get(url + '?q=' + queryParams.data.query + extraParams).then(queryParams.success);
                                res.abort = function () {
                                    return null;
                                };
                                return res;
                            },

                            results: function (data, page) {
                                var results = [];

                                _.each(data.data.response, function (obj: { id; name; disabled;}) {
                                    if (obj.disabled === false) {
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
            }
        ]);
})(angular, app);
