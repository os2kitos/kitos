(function(ng, app) {
    'use strict';

    app.directive('autofocus', [
        '$timeout', function($timeout) {
            return function(scope, elem, attr) {
                scope.$on('autofocus', function(e) {
                    $timeout(function() {
                        elem[0].focus();
                    });
                });
            };
        }
    ]);

    /* http://stackoverflow.com/questions/14833326/how-to-set-focus-in-angularjs */
    app.factory('autofocus', ['$rootScope', '$timeout', function ($rootScope, $timeout) {
        return function () {
            $timeout(function () {
                $rootScope.$broadcast('autofocus');
            });
        };
    }]);
})(angular, app);
