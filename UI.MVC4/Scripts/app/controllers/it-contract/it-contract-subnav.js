(function(ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-contract', {
            url: '/contract',
            abstract: true,
            template: '<ui-view/>',
            controller: ['$rootScope', function($rootScope) {
                $rootScope.page.title = 'IT Kontrakt';
                $rootScope.page.subnav = [
                    { state: 'it-contract.overview', text: 'Overblik: økonomi' },
                    { state: 'it-contract.plan', text: 'Overblik: tid' },
                    { state: 'it-contract.edit', text: 'IT Kontrakt', showWhen: 'it-contract.edit' },
                    { state: 'it-contract.create', text: 'Opret IT Kontrakt', asButton: 'true' }
                ];
            }]
        });
    }]);
})(angular, app);