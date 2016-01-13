(function(ng, app) {
    'use strict';

    app.directive('searchBox', [
        '$timeout', function($timeout) {
            return {
                scope: {
                    pagination: '=paging'
                },
                replace: true,
                templateUrl: 'app/shared/searchBox/searchBox.view.html',
                link: function(scope, element, attrs) {
                    var updatePromise = null;

                    function doUpdate() {
                        scope.pagination.skip = 0;
                        scope.pagination.search = scope.search;

                        updatePromise = null;
                    }


                    scope.update = function() {
                        if (updatePromise) $timeout.cancel(updatePromise);

                        updatePromise = $timeout(doUpdate, 200);
                    };
                }
            };
        }
    ]);
})(angular, app);
