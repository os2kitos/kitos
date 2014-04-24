(function (ng, app) {
    
    var subnav = [
            { state: 'index', text: 'Overblik' },
            { state: 'index', text: 'Tilknyt IT system' },
            { state: 'edit-it-system', text: 'Opret IT system' },
            { state: 'index', text: 'Rapport' }
    ];

    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('edit-it-system', {
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
                systemsHttp: ['$http', function($http) {
                    return $http.get("api/itsystem?nonInterfaces");
                }],
                interfacesHttp: ['$http', function ($http) {
                    return $http.get("api/itsystem?interfaces");
                }]
            }
        });

    }]);


    app.controller('system.AddCtrl',
        ['$rootScope', '$scope', '$http', 'notify',
            function($rootScope, $scope, $http, notify) {
                $rootScope.page.title = 'Opret IT system';
                $rootScope.page.subnav = subnav;
                


            }]);


})(angular, app);