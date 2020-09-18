((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("data-processing.edit-agreement.advice", {
            url: "/advice",
            templateUrl: "app/components/it-advice/it-advice.view.html",
            controller: "object.EditAdviceCtrl",
            controllerAs: "Vm",
            resolve: {
                Roles: ["localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                    localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.DataProcessingAgreementRoles).getAll()],
                object: ["dataProcessingAgreement", agreement => agreement],
                type: [() => "dataProcessingAgreement"],
                advicename: ["dataProcessingAgreement",
                    (dataProcessingAgreement: Kitos.Models.DataProcessing.IDataProcessingAgreementDTO) => <any>{ name: dataProcessingAgreement.name }
                ]
            }
        });
    }]);
})(angular, app);
