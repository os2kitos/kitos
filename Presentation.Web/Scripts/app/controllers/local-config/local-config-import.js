(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config.import', {
            url: '/import',
            abstract: false,
            templateUrl: 'partials/local-config/tab-import.html',
            resolve: {
                config: ['$http', 'userService', function ($http, userService) {
                    return userService.getUser().then(function (user) {
                        return user.currentConfig;
                    });
                }]
            },
            controller: ['$rootScope', '$scope', 'config',
                function($rootScope, $scope, config) {
                    $rootScope.page.importsubnav = [
                        { state: 'local-config.import.organization', text: 'Organisation' },
                        { state: 'local-config.import.users', text: 'Brugere' }
                    ];

                    $scope.config = config;
                    $scope.config.autosaveUrl = "api/config/" + config.id;
                }
            ]
        });
    }]);
})(angular, app);