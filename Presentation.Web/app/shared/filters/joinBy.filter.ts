(function(ng, app) {
    'use strict';

    // http://stackoverflow.com/questions/11540157/using-comma-as-list-separator-with-angularjs
    app.filter('joinBy', function() {
        return function(input, delimiter, displayName) {
            // default values
            delimiter = typeof delimiter !== 'undefined' ? delimiter : ', ';
            input = typeof input !== 'undefined' ? input : [];

            if (displayName) {
                var list = (_ as Kitos.ILoDashWithMixins).pluckDeep(input, displayName);
                return list.join(delimiter);
            } else {
                return (input).join(delimiter);
            }
        };
    });
})(angular, app);
