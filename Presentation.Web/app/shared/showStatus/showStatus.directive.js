(function(ng, app) {
    'use strict';

    app.directive('showStatus', [
        '$timeout', function($timeout) {
            return {
                scope: {
                    status: '=showStatus'
                },
                replace: false,
                templateUrl: 'app/shared/showStatus/showStatus.view.html',

                link: function(scope, element, attr) {
                    scope.ready = false;
                    update();

                    function update() {
                        $timeout(function() {
                            if (!scope.status) {
                                update();
                                return;
                            }
                            scope.ready = true;
                        });
                    }

                    scope.$watch("status", function(newval, oldval) {
                        if (newval === oldval) return;

                        update();
                    });
                }
            };
        }
    ]);
})(angular, app);
