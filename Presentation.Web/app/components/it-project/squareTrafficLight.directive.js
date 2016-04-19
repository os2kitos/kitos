(function(ng, app) {
    'use strict';

    app.directive('squareTrafficLight', [
        function() {
            return {
                template: '<uib-progressbar class="status-bar" data-value="value" data-type="{{type}}"></uib-progressbar>',
                scope: {
                    status: '@squareTrafficLight'
                },
                link: function(scope) {
                    switch (scope.status.toLowerCase()) {
                    case "red":
                    case 1:
                        scope.type = 'danger';
                        scope.value = 100;
                        break;
                    case "yellow":
                    case 2:
                        scope.type = 'warning';
                        scope.value = 100;
                        break;
                    case "green":
                    case 3:
                        scope.type = 'success';
                        scope.value = 100;
                        break;
                    default:
                        scope.value = 0;
                    }
                }
            };
        }
    ]);
})(angular, app);
