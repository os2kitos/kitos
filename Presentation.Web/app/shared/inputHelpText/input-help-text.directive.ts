((ng, app) => {
    "use strict";

    app.directive("inputHelpText", [
        () => ({
            templateUrl: "app/shared/inputHelpText/input-help-text.view.html",
            scope: {
                key: "@",
                cssClass: "@"
            },
            controller: [
                "$scope", ($scope) => {
                    $scope.isVisible = true;
                    if ($scope.key === "") {
                        $scope.isVisible = false;
                    }
                }]
        })
    ]);
})(angular, app);