((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("data-processing.edit-registration.advice", {
            url: "/advice",
            templateUrl: "app/components/it-advice/it-advice.view.html",
            controller: "object.EditAdviceCtrl",
            controllerAs: "Vm",
            resolve: {
                Roles: ["dataProcessingRegistrationOptions", (dataProcessingRegistrationOptions: Kitos.Models.DataProcessing.IDataProcessingRegistrationOptions) =>
                    dataProcessingRegistrationOptions.roles],
                object: ["dataProcessingRegistration", agreement => agreement],
                type: [() => "dataProcessingRegistration"],
                advicename: ["dataProcessingRegistration",
                    (dataProcessingRegistration: Kitos.Models.DataProcessing.IDataProcessingRegistrationDTO) => <any>{ name: dataProcessingRegistration.name }
                ]
            }
        });
    }]);
})(angular, app);
