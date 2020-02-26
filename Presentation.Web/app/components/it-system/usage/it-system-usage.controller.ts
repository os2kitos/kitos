((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage", {
            url: "/usage/{id:[0-9]+}",
            templateUrl: "app/components/it-system/usage/it-system-usage.view.html",
            controller: "system.UsageCtrl",
            resolve: {
                user: [
                    "userService", userService => userService.getUser()
                ],
                userAccessRights: ["authorizationServiceFactory", "$stateParams",
                    (authorizationServiceFactory: Kitos.Services.Authorization.IAuthorizationServiceFactory, $stateParams) =>
                    authorizationServiceFactory
                    .createSystemUsageAuthorization()
                    .getAuthorizationForItem($stateParams.id)
                ],
                hasWriteAccess: [
                    "userAccessRights", (userAccessRights: Kitos.Models.Api.Authorization.EntityAccessRightsDTO) => userAccessRights.canEdit
                ],
                itSystemUsage: [
                    "$http", "$stateParams", ($http, $stateParams) => $http.get("api/itSystemUsage/" + $stateParams.id)
                    .then(result => result.data.response)
                ]
            }
        });
    }]);

    app.controller("system.UsageCtrl", ["$rootScope", "$scope", "itSystemUsage", "hasWriteAccess",
        ($rootScope, $scope, itSystemUsage, hasWriteAccess) => {
            $scope.hasWriteAccess = hasWriteAccess;

            $scope.usage = itSystemUsage;

            $scope.systemUsageName = itSystemUsage.itSystem.disabled ? itSystemUsage.itSystem.name + " i " + itSystemUsage.organization.name + " (Slettes)" : itSystemUsage.itSystem.name + " i " + itSystemUsage.organization.name;

            $scope.allowClearOption = {
                allowClear: true
            };

            if (!$scope.hasWriteAccess) {
                _.remove($rootScope.page.subnav.buttons, (o:any) => o.text === "Fjern anvendelse");
            }
        }
    ]);
})(angular, app);
