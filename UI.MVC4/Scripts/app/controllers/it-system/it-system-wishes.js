(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system-usage.wishes', {
            url: '/wishes',
            templateUrl: 'partials/it-system/tab-wishes.html',
            controller: 'system.EditWishes'
        });
    }]);

    app.controller('system.EditWishes', [function () {
        
    }]);
})(angular, app);