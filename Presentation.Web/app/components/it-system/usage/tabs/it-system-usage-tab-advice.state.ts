((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.advice", {
            url: "/advice",
            templateUrl: "app/components/it-advice/it-advice.view.html",
            controller: "object.EditAdviceCtrl",
            controllerAs: "Vm",
            resolve: {
                Roles: ["localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                    localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.ItSystemRoles).getAll()],
                object: ["itSystemUsage", itSystemUsage => itSystemUsage],
                type: [() => Kitos.Models.Advice.AdviceType.ItSystemUsage],
                advicename: ["$http", "$stateParams",
                    ($http, $stateParams) => $http.get(`api/itSystemUsage/${$stateParams.id}`).then(result => {
                        var itSystemUsage = result.data.response;
                        var systemName = Kitos.Helpers.SystemNameFormat.apply(`${itSystemUsage.itSystem.name} i ${itSystemUsage.organization.name}`,itSystemUsage.itSystem.disabled);
                        return { name: systemName };
                    })
                ],
                currentUser: [
                    "userService",
                    (userService: Kitos.Services.IUserService) => userService.getUser()
                ]
            }
        });
    }]);
})(angular, app);
