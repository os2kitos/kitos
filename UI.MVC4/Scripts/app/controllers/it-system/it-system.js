(function (ng, app) {
    
    var subnav = [
            { state: 'index', text: 'Overblik' },
            { state: 'index', text: 'Tilknyt IT system' },
            { state: 'edit-it-system', text: 'Opret IT system' },
            { state: 'index', text: 'Rapport' },
    ];

    app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

        $stateProvider.state('edit-it-system', {
            url: '/system/add',
            templateUrl: 'partials/it-system/edit-system.html',
            controller: 'system.AddCtrl'
        });

    }]);


    app.controller('system.AddCtrl',
        ['$rootScope', '$scope', '$http', 'notify',
            function($rootScope, $scope, $http, notify) {
                $rootScope.page.title = 'Opret IT system';
                $rootScope.page.subnav = subnav;

            }]);


})(angular, app);