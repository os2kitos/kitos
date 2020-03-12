((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.interfaces", {
            url: "/interfaces",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-interfaces.view.html",
            controller: "systemUsage.ExposedInterfaces",
            resolve: {
                exhibits: [
                    "$http", "itSystemUsage", "user", ($http, itSystemUsage, user) =>
                        $http.get(`api/exhibit/?interfaces=true&sysId=${itSystemUsage.itSystem.id}&orgId=${user.currentOrganizationId}`).then(result => {
                            return result.data.response;
                        })
                ],
                user: [
                    "userService", userService => userService.getUser()
                ]
            }
        });
    }]);

    app.controller("systemUsage.ExposedInterfaces", ["$scope", "itSystemUsage", "exhibits",
        ($scope, itSystemUsage, exhibits) => {

            $scope.systemUsage = itSystemUsage;

            var exhibitViewModels = _.map(exhibits,
                (exhibit) => new Kitos.Models.ViewModel.ItSystem.ExposedInterfaceViewModel(
                    Kitos.Configs.ExposedInterfaceTableCellParagraphSizeConfig.maxTextFieldCharCount,
                    Kitos.Configs.ExposedInterfaceTableCellParagraphSizeConfig.shortTextLineCount,
                    exhibit));

            $scope.interfaceExposures = exhibitViewModels;

            $scope.expandParagraph = (e) => {
                Kitos.Utility.TableManipulation.expandRetractParagraphCell(e, Kitos.Configs.ExposedInterfaceTableCellParagraphSizeConfig.shortTextLineCount);
            };
        }
    ]);
})(angular, app);
