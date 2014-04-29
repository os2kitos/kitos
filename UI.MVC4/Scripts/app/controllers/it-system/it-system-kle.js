(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system-usage.kle', {
            url: '/kle',
            templateUrl: 'partials/it-system/tab-kle.html',
            controller: 'system.EditKle',
        });
    }]);

    app.controller('system.EditKle', ['$scope', function ($scope) {
        
    }]);
})(angular, app);