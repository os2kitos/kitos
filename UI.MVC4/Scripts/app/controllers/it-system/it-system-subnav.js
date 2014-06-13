(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system', {
            url: '/system',
            abstract: true,
            template: '<ui-view/>',
            controller: ['$rootScope', function ($rootScope) {
                $rootScope.page.title = 'IT System';
                $rootScope.page.subnav = [
                    { state: 'it-system.overview', text: 'Overblik' },
                    { state: 'it-system.assign', text: 'Tilknyt IT system' },
                    { state: 'it-system.add', text: 'Opret IT system' },
                ];
            }]
        });
    }]);
})(angular, app);