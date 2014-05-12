(function(ng, app) {
    'use strict';

    app.directive('holderFix', function() {
        return {
            link: function(scope, element, attrs) {
                Holder.run({ images: element[0], nocss: true });
            }
        };
    });

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
    app.factory('autofocus', ['$rootScope', '$timeout', function($rootScope, $timeout) {
        return function() {
            $timeout(function() {
                $rootScope.$broadcast('autofocus');
            });
        };
    }]);


    /* from http://stackoverflow.com/questions/11540157/using-comma-as-list-separator-with-angularjs */
    app.filter('joinBy', function () {
        return function (input, delimiter, displayName) {
            // default values
            delimiter = typeof delimiter !== 'undefined' ? delimiter : ', ';
            input = typeof input !== 'undefined' ? input : [];

            if (displayName) {
                var list = _.pluckDeep(input, displayName);
                return list.join(delimiter);
            } else {
                return (input).join(delimiter);
            }
        };
    });


})(angular, app);