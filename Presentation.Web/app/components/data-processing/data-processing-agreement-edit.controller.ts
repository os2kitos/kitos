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
                    ]
                }
            });
        }
    ]);

    app.controller("dataProcessingAgreement.EditCtrl", ["$scope", "$rootScope",
        ($scope, $rootScope) => {

        }]);
})(angular, app);