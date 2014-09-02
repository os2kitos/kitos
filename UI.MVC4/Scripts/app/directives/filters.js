(function(ng, app) {
    'use strict';

    // http://stackoverflow.com/questions/11540157/using-comma-as-list-separator-with-angularjs
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

    /**
     * filters input ids by a ids found in the sub-tree of the object specified by selectedId.
     *
     * input: list to filter
     * inputIdPropertyName: the property name in input which relates to selectedId
     * tree: an object with nested child objects of the same type, this tree also needs to be flat
     * idPropertyName: the property name in tree which relates to selectedId
     * childPropertyName: the property name of child objects in tree
     */
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