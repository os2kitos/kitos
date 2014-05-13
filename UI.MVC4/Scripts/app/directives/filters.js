(function(ng, app) {
    'use strict';

    /* from http://stackoverflow.com/questions/11540157/using-comma-as-list-separator-with-angularjs */
    app.filter('joinBy', function() {
        return function(input, delimiter, displayName) {
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

    app.filter('andChildren', function() {
        return function (input, inputIdPropertyName, tree, selectedId, idPropertyName, childPropertyName) {
            // default values
            idPropertyName = typeof idPropertyName !== 'undefined' ? idPropertyName : 'id';
            childPropertyName = typeof childPropertyName !== 'undefined' ? childPropertyName : 'children';

            if (!selectedId)
                return input; // nothing to filter by, return everything

            var foundObj = _.find(tree, function(obj) {
                return obj[idPropertyName] == selectedId;
            });

            if (foundObj) {
                var pluckedIds = _.resursivePluck(foundObj, idPropertyName, childPropertyName);
                return _.filter(input, function(obj) {
                    return _.contains(pluckedIds, obj[inputIdPropertyName]);
                });
            }
            return input;
        };
    });
})(angular, app);