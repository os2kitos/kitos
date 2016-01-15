(function(ng, app) {
    'use strict';

    app.directive('paginationButtons', [
        function() {
            return {
                scope: {
                    //the output of filtering tasks
                    pagination: "=paginationButtons",
                    totalCount: "=paginationTotalCount"
                },
                templateUrl: 'app/shared/paginationButtons/paginationButtons.view.html',
                link: function(scope, element, attrs) {
                    scope.less = function() {
                        scope.pagination.skip -= scope.pagination.take;
                        if (scope.pagination.skip < 0) scope.pagination.skip = 0;
                    };

                    scope.more = function() {
                        scope.pagination.skip += scope.pagination.take;
                    };
                }
            };
        }
    ]);
})(angular, app);
