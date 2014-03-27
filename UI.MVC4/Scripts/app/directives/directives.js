(function (ng, app) {
    app.directive('holderFix', function () {
        return {
            link: function (scope, element, attrs) {
                Holder.run({ images: element[0], nocss: true });
            }
        };
    });

    app.directive('confirmClick', [
        function() {
            return {
                link: function(scope, element, attr) {
                    var msg = attr.confirmClick || "Er du sikker?";
                    var clickAction = attr.confirmedClick;
                    element.bind('click', function(event) {
                        if (window.confirm(msg)) {
                            scope.$eval(clickAction);
                        }
                    });
                }
            };
        }]);
})(angular, app);