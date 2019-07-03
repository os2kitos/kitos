
//<editor-fold desc="polyfill for internet explorer (zombies never die :-( )">
if (typeof Object.assign !== 'function') {
    // Must be writable: true, enumerable: false, configurable: true
    Object.defineProperty(Object, "assign", {
        value: function assign(target, varArgs) { // .length of function is 2
            'use strict';
            if (target == null) { // TypeError if undefined or null
                throw new TypeError('Cannot convert undefined or null to object');
            }

            var to = Object(target);

            for (var index = 1; index < arguments.length; index++) {
                var nextSource = arguments[index];

                if (nextSource != null) { // Skip over if undefined or null
                    for (var nextKey in nextSource) {
                        // Avoid bugs when hasOwnProperty is shadowed
                        if (Object.prototype.hasOwnProperty.call(nextSource, nextKey)) {
                            to[nextKey] = nextSource[nextKey];
                        }
                    }
                }
            }
            return to;
        },
        writable: true,
        configurable: true
    });
}
//</editor-fold>

$(document).ready(function () {
    $('[data-toggle="tooltip"]').tooltip();
    setInterval(function () {
        //check if numbers in the graph are too small..then show tooltip
        var minWidth = 15;
        var checkForceTooltip = function (jselector) {
            var element$ = $(jselector);
            var eWidth = element$.width();
            if (eWidth < minWidth){
                if(element$.attr("data-trigger") !== "manual" || element$.attr("aria-describedby") === undefined){
                    element$.attr("data-trigger", "manual");
                    element$.tooltip("show");
                }
            }else{
                if(element$.attr("data-trigger") === "manual"){
                    element$.removeAttr("data-trigger");
                    element$.tooltip("hide");
                }
            }
        };
        checkForceTooltip(".progress-bar+.progress-bar-danger");
        checkForceTooltip(".progress-bar+.progress-bar-warning");

    }, 1000);
});