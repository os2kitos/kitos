﻿(function (ng, app) {
    app.config([
        '$stateProvider', function ($stateProvider) {
            $stateProvider.state('it-system.edit.main', {
                url: '/main',
                templateUrl: 'app/components/it-system/edit/tabs/it-system-edit-tab-main.view.html',
                controller: 'system.SystemMainCtrl',
                resolve: {
                    itSystem: ['$http', '$stateParams', function ($http, $stateParams) {
                        return $http.get("api/itsystem/" + $stateParams.id)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }],
                    hasWriteAccess: ['$http', '$stateParams', 'user', function ($http, $stateParams, user) {
                        return $http.get("api/itsystem/" + $stateParams.id + "?hasWriteAccess=true&organizationId=" + user.currentOrganizationId)
                            .then(function (result) {
                                return result.data.response;
                            });
                    }],
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
                    ],
                    user: [
                        'userService', function (userService) {
                            return userService.getUser();
                        }
                    ]
                }
            });
        }
    ]);

    app.controller('system.SystemMainCtrl',
        [
            '$rootScope', '$scope', '$http', '$state', 'notify', 'itSystem', 'hasWriteAccess', 'businessTypes', 'user', 'autofocus', 'appTypeOptions',
            function ($rootScope, $scope, $http, $state, notify, itSystem, hasWriteAccess, businessTypes, user, autofocus, appTypeOptions) {
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
                $scope.hasWriteAccess = hasWriteAccess;
                $scope.businessTypes = businessTypes.data.value;
                $scope.itSystemsSelectOptions = selectLazyLoading('api/itsystem', true, ['excludeId=' + itSystem.id, 'orgId=' + user.currentOrganizationId]);
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
