(function(ng, app) {
    'use strict';

    app.directive('selectStatus2', [
        '$timeout',
        function($timeout) {
            return {
                scope: {
                    canWrite: '=',
                },
                require: 'ngModel',
                templateUrl: 'app/shared/selectStatus2/selectStatus2.view.html',
                link: function(scope, element, attr, ngModel) {
                    scope.setModel = function(n) {
                        //only update on change
                        if (scope.model == n) return;

                        //save new value
                        scope.model = n;

                        $timeout(function() {
                            //then trigger event
                            ngModel.$setViewValue(scope.model);

                            //this triggers the autosave directive
                            element.triggerHandler("blur");
                        });
                    };

                    //read value from ngModel
                    ngModel.$render = function() {
                        scope.model = ngModel.$viewValue;
                    };
                }
            };
        }
    ]);
})(angular, app);
