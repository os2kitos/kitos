(function (ng, app) {
    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
        $stateProvider.state('it-system.catalog', {
            url: '/catalog',
            templateUrl: 'partials/it-system/it-system-catalog.html',
            controller: 'system.CatalogCtrl',
            resolve: {
                appTypes: [
                    '$http', function($http) {
                        return $http.get("api/apptype");
                    }
                ],
                businessTypes: [
                    '$http', function($http) {
                        return $http.get("api/businesstype");
                    }
                ],
                organizations: [
                    '$http', function($http) {
                        return $http.get("api/organization");
                    }
                ],
                systems: [
                    '$http', function($http) {
                        return $http.get("api/itsystem");
                    }
                ],
                interfaceAppType: [
                    '$http', function ($http) {
                        return $http.get("api/apptype?interfaceAppType");
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

    app.controller('system.CatalogCtrl',
        ['$rootScope', '$scope', '$http', 'notify', '$state',
            'appTypes', 'businessTypes', 'systems', 'organizations', 'interfaceAppType', 'user',
            function ($rootScope, $scope, $http, notify, $state,
             appTypesHttp, businessTypesHttp, systems, organizationsHttp, interfaceAppTypeHttp, user) {
                $rootScope.page.title = 'IT System - Katalog';

                $scope.showType = 'appType';

                var appTypes = appTypesHttp.data.response;
                var interfaceAppType = interfaceAppTypeHttp.data.response;
                var businessTypes = businessTypesHttp.data.response;
                var organizations = organizationsHttp.data.response;

                function loadUser(system) {
                    return $http.get("api/user/" + system.objectOwnerId, { cache: true })
                        .success(function (result) {
                            system.user = result.response;
                        });
                }

                function loadOrganization(system) {
                    return $http.get("api/organization/" + system.organizationId, { cache: true })
                        .success(function (result) {
                            system.organization = result.response;
                        });
                }

                function loadTaskRef(system) {
                    if (system.taskRefIds.length == 0) return null;

                    return $http.get("api/taskref/" + system.taskRefIds[0])
                        .success(function (result) {
                            system.taskId = result.response.taskKey;
                            system.taskName = result.response.description;
                        });
                }

                function loadUsage(system) {
                    return $http.get(system.usageUrl)
                        .success(function (result) {
                            system.hasUsage = true;
                        });
                }

                function addUsage(system) {
                    return $http.post("api/itsystemusage", {
                        itSystemId: system.id,
                        organizationId: user.currentOrganizationId
                    }).success(function (result) {
                        notify.addSuccessMessage("Systemet er taget i anvendelse");
                        system.hasUsage = true;
                    }).error(function (result) {
                        notify.addErrorMessage("Systemet kunne ikke tages i anvendelse!");
                    });
                }

                function deleteUsage(system) {

                    return $http.delete(system.usageUrl).success(function (result) {
                        notify.addSuccessMessage("Anvendelse af systemet er fjernet");
                        system.hasUsage = false;
                    }).error(function (result) {
                        notify.addErrorMessage("Anvendelse af systemet kunne ikke fjernes!");
                    });
                }

                $scope.systems = [];
                _.each(systems.data.response, function (system) {

                    system.appType = _.findWhere(appTypes, { id: system.appTypeId });
                    system.isInterface = (system.appTypeId == interfaceAppType.id);
                    system.businessType = _.findWhere(businessTypes, { id: system.businessTypeId });

                    system.belongsTo = _.findWhere(organizations, { id: system.belongsToId });

                    system.usageUrl = "api/itsystemusage?itSystemId=" + system.id + "&organizationId=" + user.currentOrganizationId;

                    loadUser(system);
                    loadOrganization(system);
                    loadTaskRef(system);
                    loadUsage(system);

                    system.addUsage = function() {
                        addUsage(system);
                    };

                    system.deleteUsage = function() {
                        deleteUsage(system);
                    };

                    $scope.systems.push(system);
                });
            }]);
})(angular, app);