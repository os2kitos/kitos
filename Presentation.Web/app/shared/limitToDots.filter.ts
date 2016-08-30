(function(ng, app) {
    'use strict';

    /**
     * Same as https://docs.angularjs.org/api/ng/filter/limitTo
     * But it adds ... at the end if limit is exceeded
     *
     * Only works on strings though
     */
    app.filter('limitToDots', ['limitToFilter', function (limitToFilter) {
        return function (input, limit) {
            var str = limitToFilter(input, limit);

            if (input.length > limit)
                str += '...';

            return str;
        }
    }]);
})(angular, app);
