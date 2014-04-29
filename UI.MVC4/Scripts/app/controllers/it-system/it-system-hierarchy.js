(function (ng, app) {
    app.config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('it-system-usage.hierarchy', {
            url: '/hierarchy',
            templateUrl: 'partials/it-system/tab-hierarchy.html',
            controller: 'system.EditHierarchy',
        });
    }]);

    app.controller('system.EditHierarchy', ['$scope', '$http', function ($scope, $http) {
        var curItSystemId = $scope.$parent.usage.itSystemId;
        $http.get('api/itsystem/' + curItSystemId + '?hierarchy=true')
            .then(function(result) {
                $scope.systems = toHierarchy(result.data.response, 'id', 'parentId');
            });
        
        // TODO WUFF!
        function toHierarchy(flatAry, idPropertyName, parentIdPropetyName, parentPropetyName, childPropertyName) {
            // default values
            parentPropetyName = typeof parentPropetyName !== 'undefined' ? parentPropetyName : 'parent';
            childPropertyName = typeof childPropertyName !== 'undefined' ? childPropertyName : 'children';

            // sort by parent to get roots (roots are null) first, then we only need to iterrate once
            // example [1, 1, null, 2] -> [null, 1, 1, 2] (number is parent id)
            var sorted = _.sortBy(flatAry, function (obj) {
                return obj[parentIdPropetyName];
            });

            function search(nestedAry, id) {
                if (!nestedAry || !id)
                    throw new Error("Invalid argument(s)"); // abort if not valid input

                for (var i = 0; i < nestedAry.length; i++) {
                    var obj = nestedAry[i];
                    if (obj[idPropertyName] === id) {
                        return obj;
                    } else if (obj.hasOwnProperty(childPropertyName)) { // has children, search them too
                        var found = search(obj[childPropertyName], id);
                        if (found) return found;
                    }
                }
            }

            var hierarchy = [];
            _.each(sorted, function (obj) {
                // define functions
                obj.isAllChildren = function (isChecked) {
                    if (typeof isChecked !== 'boolean')
                        throw new Error('Argument must be a boolean');

                    return _.every(this.children, function (child) {
                        if (isChecked === true) {
                            return child.selected === true;
                        } else {
                            return child.selected === false && child.indeterminate === false;
                        }
                    });
                };
                obj.setChildrenShown = function (isShown) {
                    if (typeof isShown !== 'boolean')
                        throw new Error('Argument must be a boolean');

                    var children = this.children;
                    if (!children) return;

                    _.each(children, function (child) {
                        child.show = isShown;
                        child.canWrite = isShown;
                        child.setChildrenShown(isShown);
                    });
                };
                obj.setParentShown = function (isShown) {
                    var parent = this.parent;
                    if (!parent) return;
                    parent.show = isShown;
                    parent.setParentShown(isShown);
                };
                obj.setState = function (isChecked) {
                    if (isChecked === true) {
                        this.indeterminate = false;
                        this.selected = true;
                    } else if (isChecked === false) {
                        this.indeterminate = false;
                        this.selected = false;
                    } else {
                        this.indeterminate = true;
                        this.selected = false;
                    }
                };
                obj.setChildrenState = function (isChecked) {
                    if (typeof isChecked !== 'boolean')
                        throw new Error('Argument must be a boolean');

                    var children = this.children;
                    if (!children) return;

                    _.each(children, function (child) {
                        child.setState(isChecked);
                        child.setChildrenState(isChecked);
                    });
                };
                obj.setParentState = function () {
                    var parent = this.parent;
                    if (!parent)
                        return;
                    if (parent.isAllChildren(true)) {
                        parent.setState(true);
                    } else if (parent.isAllChildren(false)) {
                        parent.setState(false);
                    } else {
                        // if all children is neither true or false 
                        // then it must be a mix
                        // so we need to set the parent as not selected 
                        // and show the indeterminate state
                        parent.setState(null);
                    }
                    // cascade up the tree
                    parent.setParentState();
                };

                if (obj[parentIdPropetyName] === null) { // is root
                    obj.level = 0;
                    hierarchy.push(obj);
                } else {
                    var parentObj = search(hierarchy, obj[parentIdPropetyName]);
                    if (!parentObj.hasOwnProperty(childPropertyName))
                        parentObj[childPropertyName] = [];

                    obj.level = parentObj.level + 1;
                    obj[parentPropetyName] = parentObj;
                    parentObj[childPropertyName].push(obj);
                }
            });

            return hierarchy;
        }
    }]);
})(angular, app);