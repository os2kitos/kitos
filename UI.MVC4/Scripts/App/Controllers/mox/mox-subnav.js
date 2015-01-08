(function(ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('mox', {
            url: '/mox',
            template: '<ui-view autoscroll="false" />',
            controller: ['$rootScope', function ($rootScope) {
                $rootScope.page.title = 'MOX';
                $rootScope.page.subnav = [
                    { state: 'mox.overview', text: 'Mox oversigt' },
                    { state: 'mox.order', text: 'Bestil' },
                    { state: 'mox.test', text: 'TEST' },
                ];
            }]
        });
    }]);
})(angular, app);
