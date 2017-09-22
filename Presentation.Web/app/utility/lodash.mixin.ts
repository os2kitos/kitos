/// <reference path="../index.d.ts" />
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
    pluckDeep: function (obj, key) {
        return _.map(obj, function (value) { return (_ as Kitos.ILoDashWithMixins).deep(value, key); });
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
    toHierarchy: function (objAry, idPropertyName, parentIdPropertyName, childPropertyName) {
        // default values
        idPropertyName = typeof idPropertyName !== 'undefined' ? idPropertyName : 'id';
        parentIdPropertyName = typeof parentIdPropertyName !== 'undefined' ? parentIdPropertyName : 'parentId';
        childPropertyName = typeof childPropertyName !== 'undefined' ? childPropertyName : 'children';

        function self(array, parent?, tree?) {
            tree = typeof tree !== 'undefined' ? tree : [];
            parent = typeof parent !== 'undefined' ? parent : { id: null };

            var children = _.filter(array, function(child) {
                return child[parentIdPropertyName] === parent[idPropertyName];
            });

            if (!_.isEmpty(children)) {
                if (parent[idPropertyName] == null) {
                    tree = children;
                } else {
                    parent[childPropertyName] = children;
                }
                _.each(children, function(child) {
                    self(array, child);
                });
            }

            return tree;
        }

        return self(objAry);
    }
});

_.mixin({
    removeFiltersForField: function (filterObj, field) {
        var clonedFilterObj = _.cloneDeep(filterObj);

        function searchAndDestory(filters) {
            // iterate backwards so we don't screw the index when removing elements
            for (var i = filters.length - 1; i >= 0; i--) {
                var filter = filters[i];

                // check if we should go deeper
                if (filter.hasOwnProperty("field")) {
                    // if field is present then we're at the max depth
                    if (filter.field == field) {
                        filters.splice(i, 1);
                    }
                } else if (filter.hasOwnProperty("filters")) {
                    // down the rabbit hole
                    searchAndDestory(filter.filters);

                    // if recursive call removed all elements in filters
                    // then we should clean up
                    if (filter.filters.length === 0) {
                        filters.splice(i, 1);
                    }
                }
            }
        }

        if (!_.isEmpty(filterObj)) {
            // kick off
            searchAndDestory(clonedFilterObj.filters);
        } else {
            // if filterObj is falsy then just return an empty object
            return {};
        }

        if (_.isEmpty(clonedFilterObj.filters)) {
            // if all filters are removed then return empty object
            return {};
        }

        return clonedFilterObj;
    }
});

_.mixin({
    addFilter: function(filterObj, field, operator, value, logic) {
        var clonedFilterObj = _.cloneDeep(filterObj);

        function findGroupByField(filters) {
            for (var i = 0; i < filters.length; i++) {
                var filter = filters[i];

                if (filter.hasOwnProperty("field")) {
                    if (filter.field == field) {
                        return filters;
                    }
                } else if (filter.hasOwnProperty("filters")) {
                    // down the rabbit hole
                    return findGroupByField(filter.filters);
                } else {
                    throw Error("field or filters property missing, this doesn't look like a filter object!");
                }
            }
            return false;
        }

        if (_.isEmpty(clonedFilterObj)) {
            // if filter object is empty, just set the filter
            clonedFilterObj = { logic: logic, filters: [{ field: field, operator: operator, value: value }] };
        } else {
            // else search for a matching filter group
            var foundGroup = findGroupByField(clonedFilterObj.filters);

            if (foundGroup) {
                // if a match is found, then add to the existing group
                foundGroup.push({ field: field, operator: operator, value: value });
            } else {
                // else create a new group
                clonedFilterObj.filters.push({ logic: logic, filters: [{ field: field, operator: operator, value: value }] });
            }
        }

        return clonedFilterObj;
    }
});

_.mixin({
    findKeyDeep: function self(obj, keyObj) {
        var p, key, val, tRet;
        for (p in keyObj) {
            if (keyObj.hasOwnProperty(p)) {
                key = p;
                val = keyObj[p];
            }
        }

        for (p in obj) {
            if (p == key) {
                if (obj[p] == val) {
                    return obj;
                }
            } else if (obj[p] instanceof Object) {
                if (obj.hasOwnProperty(p)) {
                    tRet = self(obj[p], keyObj);
                    if (tRet) { return tRet; }
                }
            }
        }

        return false;
    }
});
