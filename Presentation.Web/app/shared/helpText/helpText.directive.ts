((ng, app) => {
    "use strict";

    app.directive("helpText", [
        () => ({
            templateUrl: "app/shared/helpText/helpText.view.html",
            scope: {
                key: "@",
                defaultTitle: "@",
                noButtonLayout: "@"
            },
            controller: [
                "$scope", "$uibModal", "helpTextService", "notify", ($scope, $uibModal, helpTextService: Kitos.Services.IHelpTextService, notify) => {
                    var parent = $scope;

                    $scope.showHelpTextModal = () => {
                        $uibModal.open({
                            windowClass: "modal fade in",
                            templateUrl: "app/shared/helpText/helpTextModal.view.html",
                            controller: ["$scope", "$uibModalInstance", ($scope, $uibModalInstance) => {
                                const helpTextKey = parent.key;

                                helpTextService.loadHelpText(helpTextKey)
                                    .then(helpText => {
                                        if (helpText != null) {
                                            $scope.title = helpText.title;
                                            $scope.description = helpText.htmlText;
                                        } else {
                                            $scope.title = parent.defaultTitle;
                                            $scope.description = "Ingen hjælpetekst defineret.";

                                            helpTextService.createHelpText(helpTextKey, $scope.title);
                                        }
                                    });

                                $scope.navigateToHelpTextEdit = () => {
                                    var msg = notify.addInfoMessage("CHANGE TO DANISH: Navigating to edit page", false);

                                    helpTextService.getHelpTextFromApi(helpTextKey)
                                        .then((response) => {
                                            if (response.data.value.length < 1) {
                                                msg.toErrorMessage(`CHANGE TO DANISH: Failed to find "${helpTextKey}" help text`);
                                            }

                                            const helpText = response.data.value[0];
                                            const helpTextId = helpText.Id;
                                            window.location.href = `/#/global-admin/help-texts/edit/${helpTextId}`;

                                            $uibModalInstance.close();
                                        });
                                }
                            }]
                        });
                    }
                }]
        })
    ]);
})(angular, app);
