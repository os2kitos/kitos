(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config', {
            url: '/local-config',
            abstract: true,
            template: '<ui-view autoscroll="false" />',
            resolve: {
                config: ['$http', 'userService', function ($http, userService) {
                    return userService.getUser().then(function(user) {
                        return user.currentConfig;
                    });
                }]
            },
            controller: ['$rootScope', '$scope', 'config',
                function ($rootScope, $scope, config) {
                    $rootScope.page.title = 'Konfiguration';
                    $rootScope.page.subnav = [
                        { state: 'local-config.org', text: 'Organisation' },
                        { state: 'local-config.project', text: 'IT Projekt' },
                        { state: 'local-config.system', text: 'IT System' },
                        { state: 'local-config.contract', text: 'IT Kontrakt' },
                        { state: 'local-config.import.organization', text: 'Masse Opret'},
                        { state: 'local-config.module-admin', text: 'Module Admin'}
                    ];

                    $scope.config = config;
                    $scope.config.autosaveUrl = "api/config/" + config.id;
                }]
        });
    }]);
})(angular, app);
