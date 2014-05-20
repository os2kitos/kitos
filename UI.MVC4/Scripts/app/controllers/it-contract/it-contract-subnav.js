(function(ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract', {
            url: '/contract',
            abstract: true,
            template: '<ui-view/>',
            controller: ['$rootScope', function($rootScope) {
                $rootScope.page.title = 'IT Kontrakt';
                $rootScope.page.subnav = [
                    { state: 'it-contract.overview', text: 'Oversigt' },
                    { state: 'it-contract.plan', text: 'Udbudsplan' },
                    { state: 'it-contract.catalog', text: 'IT Kontrakt katalog' },
                    { state: 'it-contract.edit', text: 'IT Kontrakt' }
                ];
            }]
        });
    }]);
})(angular, app);