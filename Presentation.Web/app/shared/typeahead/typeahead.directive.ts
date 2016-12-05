(function (ng, app) {
    'use strict';

    app.directive('typeahead', ['$timeout', '_', '$filter', function ($timeout, _, $filter) {
        return {
            restrict: 'AEC',
            scope: {
                items: '=',
                placeholder: '@',
                displayValue: '@',
                fieldValue: '@',
                model: '=ngModel',
                stringModel: '=',
                autosaveUrl: '@',
                field: '@'
            },
            link: function (scope: any, elem, attrs) {
                if (scope.model) {
                    scope.searchInput = scope.model;
                }

                scope.handleSelection = function (selectedItem) {
                    scope.model = selectedItem[scope.fieldValue];
                    scope.searchInput = selectedItem[scope.displayValue];
                    scope.current = 0;
                };
                scope.current = 0;
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
                    scope.stringModel = scope.searchInput;
                    if (angular.isUndefined(scope.model) || scope.model.length) {
                        $timeout(function () {
                            scope.showItems = false;
                        }, 200);
                    }
                }

                scope.onKeypress = function (keyEvent) {
                    if (keyEvent.which === 38) {
                        scope.setCurrent(scope.current - 1);
                    } else if (keyEvent.which === 40) {
                        scope.setCurrent(scope.current + 1);
                    } else if (keyEvent.which === 13) {
                        var filteredItems = $filter('filter')(scope.items, scope.searchInput);
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
