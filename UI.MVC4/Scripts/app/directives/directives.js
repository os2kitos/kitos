(function (ng, app) {
    app.directive('holderFix', function () {
        return {
            link: function (scope, element, attrs) {
                Holder.run({ images: element[0], nocss: true });
            }
        };
    });
})(angular, app);