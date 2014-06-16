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
                    { state: 'it-system.catalog', text: 'IT System katalog' },
                    { state: 'it-system.edit', text: 'IT System', showWhen: 'it-system.edit' },
                    { state: 'it-system.usage', text: 'IT System anvendelse', showWhen: 'it-system.usage' }
                ];
            }]
        });
    }]);
})(angular, app);