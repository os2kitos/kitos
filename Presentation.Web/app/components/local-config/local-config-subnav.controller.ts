(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config', {
            url: '/local-config',
            abstract: true,
            template: '<ui-view autoscroll="false" />',
            resolve: {
                user: ['$http', 'userService', function ($http, userService) {
                    return userService.getUser().then(function (user) {
                        return user;
                    });
                }],

                config: ['user', function (user) {
                    return user.currentConfig;
                }]
            },
            controller: ['$rootScope', '$scope', 'config', 'user',
                function ($rootScope, $scope, config, user: Kitos.Services.IUser) {
                    $rootScope.page.title = 'Konfiguration';
                    $rootScope.page.subnav = [
                        { state: 'local-config.current-org', text: user.currentOrganizationName },
                        { state: 'local-config.org', text: 'Organisation' },
                        { state: 'local-config.project', text: 'IT Projekt' },
                        { state: 'local-config.system', text: 'IT System' },
                        { state: 'local-config.contract', text: 'IT Kontrakt' },
                        { state: 'local-config.dataProcessorAgreement', text: 'Databehandling' },
                        { state: 'local-config.import.organization', text: 'Masse Opret' }
                    ];

                    $scope.config = config;
                    $scope.config.autosaveUrl = "odata/Configs(" + config.id + ")";
                    $rootScope.subnavPositionCenter = true;

                    $scope.$on('$viewContentLoaded', function () {
                        $rootScope.positionSubnav();
                    });
                }]
        });
    }]);
})(angular, app);
