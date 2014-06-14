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
                        return $http.get("api/itsystem?nonInterfaces");
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
            'appTypes', 'businessTypes', 'systems', 'organizations', 'user',
            function ($rootScope, $scope, $http, notify, $state,
             appTypesHttp, businessTypesHttp, systems, organizationsHttp, user) {
                $rootScope.page.title = 'IT System - Katalog';

                $scope.showType = 'appType';

                var appTypes = appTypesHttp.data.response;
                var businessTypes = businessTypesHttp.data.response;
                var organizations = organizationsHttp.data.response;

                function loadUser(system) {
                    return $http.get("api/user/" + system.userId, { cache: true })
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
                            system.selected = true;
                        });
                }

                function addUsage(system) {
                    return $http.post("api/itsystemusage", {
                        itSystemId: system.id,
                        organizationId: user.currentOrganizationId
                    }).success(function (result) {
                        notify.addSuccessMessage("Systemet er tilknyttet");
                        system.selected = true;
                    }).error(function (result) {
                        notify.addErrorMessage("Systemet kunne ikke tilknyttes!");
                    });
                }

                function deleteUsage(system) {

                    return $http.delete(system.usageUrl).success(function (result) {
                        notify.addSuccessMessage("Systemet er fjernet");
                        system.selected = false;
                    }).error(function (result) {
                        notify.addErrorMessage("Systemet kunne ikke fjernes!");
                    });
                }

                $scope.systems = [];
                _.each(systems.data.response, function (system) {

                    system.appType = _.findWhere(appTypes, { id: system.appTypeId });
                    system.businessType = _.findWhere(businessTypes, { id: system.businessTypeId });

                    system.belongsTo = _.findWhere(organizations, { id: system.belongsToId });

                    system.usageUrl = "api/itsystemusage?itSystemId=" + system.id + "&organizationId=" + user.currentOrganizationId;

                    loadUser(system);
                    loadOrganization(system);
                    loadTaskRef(system);
                    loadUsage(system);

                    system.toggle = function () {
                        if (system.selected) {
                            return deleteUsage(system);
                        } else {
                            return addUsage(system);
                        }
                    };

                    $scope.systems.push(system);
                });

                $scope.create = function () {
                    var payload = {
                        name: 'Unavngivent IT system',
                        belongsToId: user.currentOrganizationId,
                        organizationId: user.currentOrganizationId,
                        userId: user.id,
                        dataRows: [],
                        taskRefIds: [],
                        canUseInterfaceIds: []
                    };

                    $scope.creatingSystem = true;
                    var msg = notify.addInfoMessage("Opretter system...", false);

                    $http.post('api/itsystem', payload).success(function (result) {

                        msg.toSuccessMessage("Et nyt system er oprettet!");

                        var systemId = result.response.id;

                        $state.go('it-system.edit', { id: systemId });

                    }).error(function () {
                        msg.toErrorMessage("Fejl! Kunne ikke oprette nyt system!");
                        $scope.creatingSystem = false;
                    });
                };


            }]);
})(angular, app);