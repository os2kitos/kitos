((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.edit.interfaces", {
            url: "/interfaces",
            templateUrl: "app/components/it-system/edit/tabs/it-system-edit-tab-interfaces.view.html",
            controller: "system.ExposedInterfaces",
            resolve: {
                exhibits: [
                    "$http", "itSystem", "user", ($http, itSystem, user) =>
                        $http.get(`api/exhibit/?interfaces=true&sysId=${itSystem.id}&orgId=${user.currentOrganizationId}`).then(result => {
                            return result.data.response;
                        })
                ],
                user: [
                    "userService", userService => userService.getUser()
                ]
            }
        });
    }]);

    app.controller("system.ExposedInterfaces", ["$scope", "itSystem", "exhibits",
        ($scope, itSystem, exhibits) => {

            $scope.system = itSystem;
            
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