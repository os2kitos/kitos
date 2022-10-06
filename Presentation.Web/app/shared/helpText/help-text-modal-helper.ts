module Kitos.Helpers {
    export class HelpTextModalHelper {
        static openHelpTextModal($uibModal: any, helpTextKey: string, helpTextService: Kitos.Services.IHelpTextService, userService: Kitos.Services.IUserService) {
            return $uibModal.open({
                windowClass: "modal fade in",
                templateUrl: "app/shared/helpText/helpTextModal.view.html",
                controller: ["$scope", "$uibModalInstance", ($scope, $uibModalInstance) => {

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
                                $scope.title = helpTextKey;
                                $scope.description = "Ingen hjælpetekst defineret (skal oprettes af Global Admin)";
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
    }
}