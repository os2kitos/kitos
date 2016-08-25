(function(ng, app) {
    'use strict';

    app.directive('showErrors', [
            '$timeout', function($timeout) {
                var linkFn;
                linkFn = function(scope, el, attrs, formCtrl) {
                    var blurred, inputEl, inputName, inputNgEl, showSuccess, toggleClasses;
                    blurred = false;
                    showSuccess = true;
                    inputEl = el[0].querySelector('[name]');
                    inputNgEl = angular.element(inputEl);
                    inputName = inputNgEl.attr('name');
                    if (!inputName) {
                        throw 'show-errors element has no child input elements with a \'name\' attribute';
                    }
                    inputNgEl.bind('blur', function() {
                        blurred = true;
                        return toggleClasses(formCtrl[inputName].$invalid);
                    });
                    scope.$watch(function() {
                        return formCtrl[inputName] && formCtrl[inputName].$invalid;
                    }, function(invalid) {
                        if (!blurred) {
                            return;
                        }
                        return toggleClasses(invalid);
                    });
                    scope.$on('show-errors-check-validity', function() {
                        return toggleClasses(formCtrl[inputName].$invalid);
                    });
                    scope.$on('show-errors-reset', function() {
                        return $timeout(function() {
                            el.removeClass('has-error');
                            el.removeClass('has-success');
                            return blurred = false;
                        }, 0, false);
                    });
                    return toggleClasses = function(invalid) {
                        el.toggleClass('has-error', invalid);
                        if (showSuccess) {
                            return el.toggleClass('has-success', !invalid);
                        }
                    };
                };
                return {
                    restrict: 'A',
                    require: '^form',
                    compile: function(elem, attrs) {
                        if (!elem.hasClass('form-group')) {
                            throw 'show-errors element does not have the \'form-group\' class';
                        }
                        return linkFn;
                    }
                };
            }
        ]
    );
})(angular, app);
