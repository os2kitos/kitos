(function (ng, app) {


    app.config(['$stateProvider', '$urlRouterProvider', function($stateProvider, $urlRouterProvider) {

        $stateProvider.state('it-system.usage', {
            url: '/usage/{id:[0-9]+}',
            templateUrl: 'partials/it-system/usage-it-system.html',
            controller: 'system.UsageCtrl',
            resolve: {
                appTypes: [
                    '$http', function($http) {
                        return $http.get("api/apptype")
                            .then(function(result) {
                                return result.data.response;
                            });
                    }
                ],
                businessTypes: [
                    '$http', function($http) {
                        return $http.get("api/businesstype")
                            .then(function(result) {
                                return result.data.response;
                            });
                    }
                ],
                archiveTypes: [
                    '$http', function($http) {
                        return $http.get("api/archivetype")
                            .then(function(result) {
                                return result.data.response;
                            });
                    }
                ],
                sensitiveDataTypes: [
                    '$http', function($http) {
                        return $http.get("api/sensitivedatatype")
                            .then(function(result) {
                                return result.data.response;
                            });
                    }
                ],
                itSystems: [
                    '$http', function($http) {
                        return $http.get("api/itsystem/")
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
                interfaceAppType: [
                    '$http', function($http) {
                        return $http.get("api/apptype?interfaceAppType").then(function(result) {
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

    app.controller('system.UsageCtrl', ['$rootScope', '$scope', '$http', '$stateParams', 'notify', 'itSystemUsage', 'appTypes',
        'businessTypes', 'archiveTypes', 'sensitiveDataTypes', 'itSystems', 'interfaceAppType', 'hasWriteAccess', 'autofocus',
        function ($rootScope, $scope, $http, $stateParams, notify, itSystemUsage, appTypes, businessTypes, archiveTypes, sensitiveDataTypes, itSystems, interfaceAppType, hasWriteAccess, autofocus) {
            $rootScope.page.title = 'IT System - Anvendelse';

            autofocus();

            //TODO create tab for this page
            //$rootScope.page.subnav = subnav.slice();
            //$rootScope.page.subnav.push({ state: 'it-system.usage', text: 'IT System' });

            $scope.hasWriteAccess = hasWriteAccess;

            $scope.interfaceAppType = interfaceAppType;

            $scope.usageId = $stateParams.id;
            $scope.status = [{ id: true, name: 'Aktiv' }, { id: false, name: 'Inaktiv' }];
            $scope.appTypes = appTypes;
            $scope.businessTypes = businessTypes;
            $scope.archiveTypes = archiveTypes;
            $scope.sensitiveDataTypes = sensitiveDataTypes;
            $scope.itSystems = itSystems;
            $scope.usage = itSystemUsage;

            if (itSystemUsage.itSystem.parentId) {
                $scope.parentSystem = $http.get('api/itsystem/' + itSystemUsage.itSystem.parentId).then(function(result) {
                    return result.data.response;
                });
            }

            $scope.orgUnits = itSystemUsage.usedBy;
        }
    ]);
})(angular, app);