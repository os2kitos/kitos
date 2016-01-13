(function(ng, app) {
    'use strict';

    app.directive('hrefBlank', ['$window', function ($window) {
        return {
            restrict: 'A',
            scope: {
                url: '=hrefBlank'
            },
            link: function (scope, element, attrs) {
                element.bind('click', function () {
                    $window.open(scope.url, '_blank');
                });
            }
        };
    }]);
})(angular, app);
