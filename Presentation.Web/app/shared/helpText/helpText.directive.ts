((ng, app) => {
    "use strict";

    app
        .directive("helpText", [
            () => ({
                templateUrl: "app/shared/helpText/helpText.view.html",
                scope: {
                    key: "@",
                    defaultTitle: "@",
                    noButtonLayout: "@"
                },
                controller: [
                    "$scope", "$uibModal", "helpTextService", "userService", ($scope, $uibModal, helpTextService: Kitos.Services.IHelpTextService, userService) => {
                        var parent = $scope;

                        $scope.showHelpTextModal = () => {
                            $uibModal.open({
                                windowClass: "modal fade in",
                                templateUrl: "app/shared/helpText/helpTextModal.view.html",
                                controller: ["$scope", "$uibModalInstance", ($scope, $uibModalInstance) => {
                                    const helpTextKey = parent.key;

                                    userService.getUser()
                                        .then(user => {
                                            $scope.canEditHelpTexts = user.isGlobalAdmin;
                                        });

                                    helpTextService.loadHelpText(helpTextKey)
                                        .then(helpText => {
                                            if (helpText != null) {
                                                $scope.title = helpText.title;
                                                $scope.description = helpText.htmlText;
                                            } else {
                                                $scope.title = parent.defaultTitle;
                                                $scope.description = "Ingen hjælpetekst defineret.";
                                            }
                                        });

                                    $scope.navigateToHelpTextEdit = () => {
                                        helpTextService.loadHelpText(helpTextKey, true)
                                            .then((helpText) => {
                                                if (helpText === null) {
                                                    return createHelpTextAndNavigateToEdit(helpTextKey);
                                                } else {
                                                    return navigateToEdit(helpText.id);
                                                }
                                            });
                                    }

                                    function createHelpTextAndNavigateToEdit(key: string) {
                                        helpTextService.createHelpText(key, $scope.title).then((response: any) => {
                                            if (response.data?.Id !== undefined) {
                                                navigateToEdit(response.data.Id);
                                            }
                                        });
                                    }

                                    function navigateToEdit(id: number) {
                                        window.location.href = `/#/global-admin/help-texts/edit/${id}`;

                                        $uibModalInstance.close();
                                    }
                                }]
                            });
                        }
                    }]
            })
        ]);
})(angular, app);
