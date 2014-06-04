(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config', {
            url: '/local-config/{id:[0-9]+}',
            abstract: true,
            template: '<ui-view/>',
            resolve: {
                config: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get('api/config/' + $stateParams.id).then(function(result) {
                        return result.data.response;
                    });
                }]
            },
            controller: ['$rootScope', function ($rootScope) {
                $rootScope.page.title = 'Konfiguration';
                $rootScope.page.subnav = [
                    { state: 'local-config.org', text: 'Organisation' },
                    { state: 'local-config.project', text: 'IT Projekt' },
                    { state: 'local-config.system', text: 'IT System' },
                    { state: 'local-config.contract', text: 'IT Kontrakt' }
                ];
            }]
        });
    }]);
})(angular, app);