(function(ng, app) {
    'use strict';

    app.directive('orderBy', [
        function() {
            return {
                scope: {
                    orderBy: '=orderBy',
                    pagination: '=paging',
                },
                replace: true,
                templateUrl: 'app/shared/orderBy/orderBy.view.html',
                link: function(scope, element, attrs) {
                    scope.order = function() {
                        scope.pagination.skip = 0;

                        if (scope.pagination.orderBy == scope.orderBy) {
                            scope.pagination.descending = !scope.pagination.descending;
                        } else {
                            scope.pagination.orderBy = scope.orderBy;
                            scope.pagination.descending = false;
                        }
                    };
                }

            };

        }
    ]);
})(angular, app);
