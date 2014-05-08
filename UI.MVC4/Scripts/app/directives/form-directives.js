(function (ng, app) {
    'use strict';

    //Match two input fields against each other
    //Usage <input ng-model="firstModel" match="otherModel">
    app.directive('match', [
        function () {
            return {
                restrict: 'A',
                scope: true,
                require: 'ngModel',
                link: function (scope, elem, attrs, control) {
                    var checker = function () {

                        //get the value of the first password
                        var e1 = scope.$eval(attrs.ngModel);

                        //get the value of the other password  
                        var e2 = scope.$eval(attrs.match);
                        return e1 == e2;
                    };
                    scope.$watch(checker, function (n) {

                        //set the form control to valid if both 
                        //passwords are the same, else invalid
                        control.$setValidity("unique", n);
                    });
                }
            };
        }
    ]);

    //Opens a dialog; if the user confirms, the action given
    //by the attribute 'confirmed-click' is run
    app.directive('confirmClick', [
    function () {
        return {
            link: function (scope, element, attr) {
                var msg = attr.confirmClick || "Er du sikker?";
                var clickAction = attr.confirmedClick;
                element.bind('click', function (event) {
                    if (window.confirm(msg)) {
                        scope.$eval(clickAction);
                    }
                });
            }
        };
    }]);

    //If the user specifies handleBusy as an option to $http,
    //any button, input etc will be disabled during that $http call
    //Useful to prevent double submit
    app.directive('disabledOnBusy', [
    function () {
        return function (scope, elem, attr) {
            scope.$on('httpBusy', function (e) {
                elem[0].disabled = true;
            });

            scope.$on('httpUnbusy', function (e) {
                elem[0].disabled = false;
            });
        };
    }
    ]);
})(angular, app);