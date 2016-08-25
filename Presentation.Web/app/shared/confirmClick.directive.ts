(function(ng, app) {
    "use strict";

    // opens a dialog; if the user confirms, the action given
    // by the attribute "confirmed-click" is run
    app.directive("confirmClick", [
        function() {
            return {
                link: function(scope, element, attr) {
                    var msg = attr.confirmClick || "Er du sikker?";
                    var clickAction = attr.confirmedClick;
                    element.bind("click", function(event) {
                        if (window.confirm(msg)) {
                            scope.$eval(clickAction);
                        }
                    });
                }
            };
        }
    ]);
})(angular, app);
