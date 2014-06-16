(function(ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-project', {
            url: '/project',
            abstract: true,
            template: '<ui-view/>',
            controller: ['$rootScope', function($rootScope) {
                $rootScope.page.title = 'IT Projekt';
                $rootScope.page.subnav = [
                    { state: 'it-project.overview', text: 'Overblik' },
                    { state: 'it-project.portfolio', text: 'Portefølje' },
                    { state: 'it-project.catalog', text: 'IT Projekt katalog' },
                    { state: 'it-project.edit', text: 'IT Projekt', showWhen: 'it-project.edit' }
                ];
            }]
        });
    }]);
})(angular, app);