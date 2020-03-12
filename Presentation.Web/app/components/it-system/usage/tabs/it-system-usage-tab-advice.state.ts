((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.advice", {
            url: "/advice",
            templateUrl: "app/components/it-advice/it-advice.view.html",
            controller: "object.EditAdviceCtrl",
            controllerAs: "Vm",
            resolve: {
                Roles: ["$http", $http => $http.get("odata/LocalItSystemRoles?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                    .then(result => result.data.value)],
                advices: ["$http", "$stateParams", ($http, $stateParams) => $http.get(`api/itSystemUsage/${$stateParams.id}`).then(result => result.data.response.advices)],
                object: ["itSystemUsage", itSystemUsage => itSystemUsage],
                type: [() => "itSystemUsage"],
                advicename: ["$http", "$stateParams",
                    ($http, $stateParams) => $http.get(`api/itSystemUsage/${$stateParams.id}`).then(result => {
                        var itSystemUsage = result.data.response;
                        var systemName = itSystemUsage.itSystem.disabled ? itSystemUsage.itSystem.name + " i " + itSystemUsage.organization.name + " (Slettes)" : itSystemUsage.itSystem.name + " i " + itSystemUsage.organization.name;
                        return { name: systemName };
                    })
                ]
            }
        });
    }]);
})(angular, app);
