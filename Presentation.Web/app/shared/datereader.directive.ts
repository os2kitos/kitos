(function(ng, app) {
    'use strict';

    app.directive('datereader', ['moment',
        function (moment) {
            return {
                scope: true,
                template: '<span>{{dateStr}}</span>',
                require: 'ngModel',
                link: function(scope, element, attr, ctrl) {

                    scope.date = {};

                    function read() {
                        if (angular.isUndefined(ctrl.$modelValue) || ctrl.$modelValue == null) scope.dateStr = "";
                        else scope.dateStr = moment(ctrl.$modelValue).format("DD-MM-YY", "da", true);
                    }

                    read();
                    ctrl.$render = read;
                }
            };
        }
    ]);
})(angular, app);
