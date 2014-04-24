(function (ng, app) {
    
    var subnav = [
            { state: 'index', text: 'Overblik' },
            { state: 'index', text: 'Tilknyt IT system' },
            { state: 'add-it-system', text: 'Opret IT system' },
            { state: 'index', text: 'Rapport' }
    ];

    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('add-it-system', {
            url: '/system/add',
            templateUrl: 'partials/it-system/edit-system.html',
            controller: 'system.AddCtrl',
            resolve: {
                appTypes: ['$http', function($http) {
                    return $http.get("api/apptype");
                }],
                businessTypes: ['$http', function ($http) {
                    return $http.get("api/businesstype");
                }],
                tsas: ['$http', function ($http) {
                    return $http.get("api/tsa");
                }],
                interfaces: ['$http', function ($http) {
                    return $http.get("api/interface");
                }],
                interfaceTypes: ['$http', function ($http) {
                    return $http.get("api/interfacetype");
                }],
                methods: ['$http', function ($http) {
                    return $http.get("api/method");
                }]
            }
        });

    }]);


    app.controller('system.AddCtrl',
        ['$rootScope', '$scope', '$http', 'notify',
            'appTypes', 'businessTypes', 'tsas', 'interfaces', 'interfaceTypes', 'methods',
            function ($rootScope, $scope, $http, notify,
            appTypes, businessTypes, tsas, interfaces, interfaceTypes, methods) {
                $rootScope.page.title = 'Opret IT system';
                $rootScope.page.subnav = subnav;

                $scope.appTypes = appTypes.data.response;
                $scope.businessTypes = businessTypes.data.response;
                
                $scope.tsas = tsas.data.response;
                $scope.interfaces = interfaces.data.response;
                $scope.interfaceTypes = interfaceTypes.data.response;
                $scope.methods = methods.data.response;

                $scope.itSystemsSelectOptions = systemLazyLoading('nonInterfaces');
                $scope.itSystemsInterfacesOptions = systemLazyLoading('interfaces', true);
                

                function systemLazyLoading(urlExtra, multiple) {
                    return {
                        multiple: multiple,
                        minimumInputLength: 1,
                        initSelection: function (elem, callback) {
                        },
                        ajax: {
                            data: function (term, page) {
                                return { query: term };
                            },
                            quietMillis: 500,
                            transport: function (queryParams) {
                                var res = $http.get('api/itsystem?q=' + queryParams.data.query + "&" + urlExtra).then(queryParams.success);
                                res.abort = function () {
                                    return null;
                                };

                                return res;
                            },

                            results: function (data, page) {
                                var results = [];

                                _.each(data.data.response, function (system) {

                                    results.push({
                                        id: system.id,
                                        text: system.name
                                    });
                                });

                                return { results: results };
                            }
                        }
                    };
                }
            }]);


})(angular, app);