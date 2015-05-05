(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('local-config.import', {
            url: '/import',
            abstract: true,
            templateUrl: 'partials/local-config/tab-import.html',
            resolve: {
                user: [
                    'userService', function (userService) {
                        return userService.getUser();
                    }
                ]
            },
            controller: ['$rootScope',
                function ($rootScope) {
                    $rootScope.page.importsubnav = [
                        { state: 'local-config.import.organization', text: 'Organisation' },
                        { state: 'local-config.import.users', text: 'Brugere' },
                        { state: 'local-config.import.interfaces', text: 'Snitflader' },
                        { state: 'local-config.import.contracts', text: 'IT Kontrakter' }
                    ];
                }
            ]
        });
    }]);
})(angular, app);
