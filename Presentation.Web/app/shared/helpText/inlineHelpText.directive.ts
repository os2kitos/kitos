((ng, app) => {
    "use strict";

    app.directive("inlineHelpText", [
        () => ({
            templateUrl: "app/shared/helpText/inlineHelpText.view.html",
            scope: {
                key: "@",
                defaultTitle: "@",
                defaultText: "@"
            },
            controller: [
                "$scope", "helpTextService", ($scope, helpTextService: Kitos.Services.IHelpTextService) => {
                    helpTextService.loadHelpText($scope.key)
                        .then(helpText => {
                            const defaultTitle = $scope.defaultTitle;
                            const defaultText = $scope.defaultText;
                            if (helpText != null) {
                                $scope.title = helpText.title ?? defaultTitle;
                                $scope.text = helpText.htmlText ?? defaultText;
                            } else {
                                $scope.title = defaultTitle;
                                $scope.text = defaultText;
                            }
                        });
                }]
        })
    ]);
})(angular, app);
