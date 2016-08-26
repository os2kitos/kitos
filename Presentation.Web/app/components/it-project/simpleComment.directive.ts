(function(ng, app) {
    'use strict';

    app.directive('simpleComment', [
        function() {
            return {
                scope: true,
                require: 'ngModel',
                template: '<button class="btn btn-link btn-sm" data-ng-disabled="disabled" data-popover="{{comment}}"><i class="glyphicon glyphicon-comment small" data-ng-class="ngClassObj"></i></button>',
                link: function(scope, element, attr, ctrl) {

                    function setDisabled(disabled) {
                        scope.disabled = disabled;
                        scope.ngClassObj = { 'faded': disabled };
                    }

                    setDisabled(true);

                    ctrl.$render = function() {
                        setDisabled(!ctrl.$viewValue);

                        scope.comment = ctrl.$viewValue;
                    };
                }
            };
        }
    ]);
})(angular, app);
