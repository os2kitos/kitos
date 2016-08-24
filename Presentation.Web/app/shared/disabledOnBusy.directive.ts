(function (ng, app) {
    "use strict";

    // if the user specifies handleBusy as an option to $http,
    // any button, input etc will be disabled during that $http call
    // useful to prevent double submit
    app.directive("disabledOnBusy", [
    function () {
        return function (scope, elem, attr) {
            scope.$on("httpBusy", function (e) {
                elem[0].disabled = true;
            });

            scope.$on("httpUnbusy", function (e) {
                elem[0].disabled = false;
            });
        };
    }
    ]);
})(angular, app);
