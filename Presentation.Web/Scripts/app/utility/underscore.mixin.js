// Usage:
//
// var obj = {
//   a: {
//     b: {
//       c: {
//         d: ['e', 'f', 'g']
//       }
//     }
//   }
// };
//
// Get deep value
// _.deep(obj, 'a.b.c.d[2]'); // 'g'
//
// Set deep value
// _.deep(obj, 'a.b.c.d[2]', 'george');
//
// _.deep(obj, 'a.b.c.d[2]'); // 'george'
_.mixin({
    // Get/set the value of a nested property
    deep: function(obj, key, value) {

        var keys = key.replace(/\[(["']?)([^\1]+?)\1?\]/g, '.$2').replace(/^\./, '').split('.'),
            root,
            i = 0,
            n = keys.length;

        // Set deep value
        if (arguments.length > 2) {

            root = obj;
            n--;

            while (i < n) {
                key = keys[i++];
                obj = obj[key] = _.isObject(obj[key]) ? obj[key] : {};
            }

            obj[keys[i]] = value;

            value = root;

            // Get deep value
        } else {
            while ((obj = obj[keys[i++]]) != null && i < n) {
            }
            ;
            value = i < n ? void 0 : obj;
        }

        return value;
    }
});


// Usage:
//
// var arr = [{
//   deeply: {
//     nested: 'foo'
//   }
// }, {
//   deeply: {
//     nested: 'bar'
//   }
// }];
// 
// _.pluckDeep(arr, 'deeply.nested'); // ['foo', 'bar']
_.mixin({
    pluckDeep: function(obj, key) {
        return _.map(obj, function(value) { return _.deep(value, key); });
    }
});


_.mixin({

    // Return a copy of an object containing all but the blacklisted properties.
    unpick: function (obj) {
        obj || (obj = {});
        return _.pick(obj, _.difference(_.keys(obj), _.flatten(Array.prototype.slice.call(arguments, 1))));
    }

});

_.mixin({   
    resursivePluck: function self(obj, key, childPropertyName) {
        // default values
        childPropertyName = typeof childPropertyName !== 'undefined' ? childPropertyName : 'children';

        var result = [];
        result.push(obj[key]);

        if (obj.hasOwnProperty(childPropertyName)) {
            _.each(obj[childPropertyName], function(item) {
                result.pushArray(self(item, key, childPropertyName));
            });
        }
        return result;
    }
});

// Usage: 
// var obj = [{
//     id: 1, children: [
//         { id: 2 },
//         { id: 3 }
//     ]
// }]
// 
// _.addHierarchyLevel(obj, 0, 'children');
// Result:
// var obj = [{
//     id: 1, $level: 0, children: [
//         { id: 2, $level: 1 },
//         { id: 3, $level: 1 }
//     ]
// }]

_.mixin({
    addHierarchyLevelOnNested: function self(objAry, level, childPropertyName) {
        // default values
        level = typeof level !== 'undefined' ? level : 0;
        childPropertyName = typeof childPropertyName !== 'undefined' ? childPropertyName : 'children';

        _.forEach(objAry, function(obj) {
            // add level attribute
            obj["$level"] = level;

            // handle children
            if (obj.hasOwnProperty(childPropertyName) && obj[childPropertyName] !== null) {
                // child object(s)
                var children = obj[childPropertyName];
                if (!_.isArray(children))
                    children = [children];

                _.forEach(children, function(child) {
                    self([child], level + 1, childPropertyName);
                });
            }
        });
    }
});

// http://stackoverflow.com/questions/30864656/set-depth-level-properity-in-flattened-object-hierarchy
_.mixin({
    addHierarchyLevelOnFlatAndSort: function(objAry, idPropertyName, parentIdPropertyName) {
        // default values
        idPropertyName = typeof idPropertyName !== 'undefined' ? idPropertyName : 'id';
        parentIdPropertyName = typeof parentIdPropertyName !== 'undefined' ? parentIdPropertyName : 'parentId';

        // clone to avoid changing source
        var clone = _.clone(objAry);
        var sorted = [];

        // TODO rewrite using lodash
        function setLevel(parentId, level) {
            var ids = [];
            clone.filter(function (el) {
                return el[parentIdPropertyName] === parentId;
            }).forEach(function (el) {
                // add depth level to element
                el.$level = level;
                // add to "sorted" result array
                // this is to ensure that children comes directly after their parent,
                // handy when displaying
                sorted.push(el);
                // recursive call to set child levels and also return their ids
                el.childIds = setLevel(el[idPropertyName], level + 1);
                // so we can add child ids for easy lookup, as they're needed for OData filtering
                ids.push(el[idPropertyName]);
                ids.pushArray(el.childIds);
            });

            return ids;
        }

        // starts the recursive call at the parent level
        setLevel(null, 0);

        return sorted;
    }
});

_.mixin({
    removeFiltersForField: function (filter, field) {
        // clone to avoid changing source
        var clone = _.clone(filter);

        var isNested = !clone.filters[0].hasOwnProperty("field");
        if (isNested) {
            // iterrate backwards so we can remove items along the way
            _.remove(clone.filters, function(n) {
                return n.filters.field === field;
            });
            
        } else {
            if (clone.filters[0].field === field) {
                clone = {};
            }
        }

        return clone;
    }
});
