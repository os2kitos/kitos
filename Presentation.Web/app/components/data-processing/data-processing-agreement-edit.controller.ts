((ng, app) => {
    app.config([
        "$stateProvider", $stateProvider => {
            $stateProvider.state("data-processing.edit-agreement", {
                url: "/edit/{id:[0-9]+}",
                templateUrl: "app/components/data-processing/data-processing-agreement-edit.view.html",
                controller: "dataProcessingAgreement.EditCtrl",
                controllerAs: "dataProcessingAgreementEditVm",
                resolve: {
                    user: ["userService", userService => userService.getUser().then(user => user)
                    ],
                    userAccessRights: ["authorizationServiceFactory", "$stateParams",
                        (authorizationServiceFactory: Kitos.Services.Authorization.IAuthorizationServiceFactory, $stateParams) =>
                            authorizationServiceFactory.createDataProcessingAgreementAuthorization().getAuthorizationForItem($stateParams.id)
                    ],
                    hasWriteAccess: [
                        "userAccessRights", (userAccessRights: Kitos.Models.Api.Authorization.EntityAccessRightsDTO) => userAccessRights.canEdit
                    ],
                    dataProcessingAgreement: ['$http', '$stateParams', ($http, $stateParams) => $http.get("api/v1/data-processing-agreement/" + $stateParams.id)
                        .then(result => result.data.response)],
                }
            });
        }
    ]);

    app.controller("dataProcessingAgreement.EditCtrl", ["$scope", "$rootScope", "userAccessRights",
        ($scope, $rootScope, userAccessRights) => {
            if (!userAccessRights.canDelete) {
                _.remove($rootScope.page.subnav.buttons, function (o: any) {
                    return o.text === "Slet Databehandlingsaftale";
                });
            }
        }]);
})(angular, app);