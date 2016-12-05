(function (ng, app) {
    'use strict';

    app.directive('typeahead', ['$timeout', '_', '$filter', function ($timeout, _, $filter) {
        return {
            restrict: 'AEC',
            scope: {
                items: '=',
                prompt: '@',
                title: '@',
                subtitle: '@',
                property: '@',
                model: '=ngModel',
                stringValue: '=',
                onSelect: '&',
                autosaveUrl: '@',
                field: '@'
            },
            link: function (scope: any, elem, attrs) {

                if (scope.model) {
                    var selectedItem = _.find(scope.items, { Id: scope.model });
                    scope.searchInput = selectedItem.Name;
                }

                scope.handleSelection = function (selectedItem) {
                    scope.model = selectedItem[scope.property];
                    scope.searchInput = selectedItem[scope.title];
                    scope.current = 0;
                    scope.selected = true;
                    $timeout(function () {
                        scope.onSelect();
                    }, 200);
                };
                scope.current = 0;
                scope.selected = true; // hides the list initially
                scope.isCurrent = function (index) {
                    return scope.current == index;
                };
                scope.setCurrent = function (index) {
                    if (scope.items[index]) {
                        scope.current = index;
                    }
                };

                scope.onFocus = function () {
                    scope.showItems = true;
                }

                scope.onBlur = function () {
                    scope.stringValue = scope.searchInput;
                    if (angular.isUndefined(scope.model) || !scope.model.length) {
                        $timeout(function () {
                            scope.showItems = false;
                        }, 200);
                    }
                }

                scope.onKeypress = function (keyEvent) {
                    if (keyEvent.which === 38) {
                        scope.setCurrent(scope.current - 1)
                    } else if (keyEvent.which === 40) {
                        scope.setCurrent(scope.current + 1);
                    } else if (keyEvent.which === 13) {
                        var filteredItems = $filter('filter')(scope.items, scope.searchInput);
                        console.log(filteredItems)
                        if (filteredItems.length) {
                            scope.handleSelection(filteredItems[scope.current]);
                        }
                        elem.find('.typeahead-input').blur();
                    }
                }
            },
            templateUrl: 'app/shared/typeahead/typeahead.view.html'
        };
    }]);
})(angular, app);
