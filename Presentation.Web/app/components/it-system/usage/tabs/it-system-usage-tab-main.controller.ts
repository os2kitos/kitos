((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.main", {
            url: "/main",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-main.view.html",
            controller: "system.EditMain",
            resolve: {
                systemCategories: [
                    "localOptionServiceFactory", (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) =>
                        localOptionServiceFactory.create(Kitos.Services.LocalOptions.LocalOptionType.ItSystemCategories).getAll()
                ],
                itSystemUsage: [
                    "itSystemUsageService", "$stateParams", (itSystemUsageService: Kitos.Services.ItSystemUsage.IItSystemUsageService, $stateParams) => itSystemUsageService.getItSystemUsage($stateParams.id)
                ]
            }
        });
    }]);

    app.controller("system.EditMain", ["$rootScope", "$scope", "$http", "notify", "user", "systemCategories",
        "autofocus", "itSystemUsageService", "select2LoadingService", "uiState", "itSystemUsage",
        ($rootScope, $scope, $http, notify, user, systemCategories, autofocus,
            itSystemUsageService: Kitos.Services.ItSystemUsage.IItSystemUsageService,
            select2LoadingService: Kitos.Services.ISelect2LoadingService,
            uiState: Kitos.Models.UICustomization.ICustomizedModuleUI,
            itSystemUsage) => {
            var itSystemUsageVm = new Kitos.Models.ViewModel.ItSystemUsage.SystemUsageViewModel(itSystemUsage);
            $scope.usage = itSystemUsage;
            const blueprint = Kitos.Models.UICustomization.Configs.BluePrints.ItSystemUsageUiCustomizationBluePrint;

            $rootScope.page.title = "IT System - Anvendelse";
            $scope.autoSaveUrl = `api/itSystemUsage/${itSystemUsageVm.id}`;
            $scope.hasViewAccess = user.currentOrganizationId === itSystemUsageVm.organizationId;
            $scope.systemCategories = systemCategories;
            $scope.shouldShowCategories = systemCategories.length > 0;
            $scope.system = itSystemUsageVm.itSystem;
            $scope.lastChangedBy = itSystemUsageVm.lastChangedBy;
            $scope.lastChanged = itSystemUsageVm.lastChanged;
            autofocus();
            $scope.isValidUrl = (url: string) => Kitos.Utility.Validation.isValidExternalReference(url);
            const saveUrlWithOrgId = $scope.autoSaveUrl + "?organizationId=" + user.currentOrganizationId;

            $scope.showUsagePeriod = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage.children.usagePeriod);
            $scope.showLifeCycleStatus = uiState.isBluePrintNodeAvailable(blueprint.children.frontPage.children.lifeCycleStatus);

            $scope.numberOfUsersOptions = [
                { id: "4", text: Kitos.Constants.Select2.EmptyField },
                { id: "0", text: "<10" },
                { id: "1", text: "10-50" },
                { id: "2", text: "50-100" },
                { id: "3", text: ">100" }
            ];
            
            $scope.datepickerOptions = Kitos.Configs.standardKendoDatePickerOptions;

            bindLifeCycleStatusModel();
            reloadValidationStatus();

            $scope.patchDate = (field, value) => {

                var expirationDate = $scope.usage.expirationDate;
                var concluded = $scope.usage.concluded;

                if (!value) {
                    var payload = {};
                    payload[field] = null;
                    patch(payload, saveUrlWithOrgId);
                }
                else if (Kitos.Helpers.DateValidationHelper.validateValidityPeriod(concluded, expirationDate, notify, "Ibrugtagningsdato", "Slutdato for anvendelse")) {
                    const dateString = moment(value, [Kitos.Constants.DateFormat.DanishDateFormat, Kitos.Constants.DateFormat.EnglishDateFormat]).format(Kitos.Constants.DateFormat.EnglishDateFormat);
                    var payload = {};
                    payload[field] = dateString;
                    patch(payload, saveUrlWithOrgId);
                }
            }

            function patch(payload: any, url: any);
            function patch(payload, url) {
                var msg = notify.addInfoMessage("Gemmer...", false);
                $http({ method: "PATCH", url: url, data: payload })
                    .then(function onSuccess(result) {
                        msg.toSuccessMessage("Feltet er opdateret.");
                        reloadValidationStatus();
                    }, function onError(result) {
                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                    });
            }

            function reloadValidationStatus() {
                itSystemUsageService.getValidationDetails(itSystemUsageVm.id).then(newStatus => {
                    $scope.validationStatus = newStatus;
                });
            }

            function bindLifeCycleStatusModel() {
                const lifeCycleStatusOptions = new Kitos.Models.ItSystemUsage.LifeCycleStatusOptions();
                const options = lifeCycleStatusOptions.options;
                const currentId = itSystemUsageVm.lifeCycleStatus ?? 0;
                const optionsWithCurrentId = options.filter(x => x.id === currentId);
                if (!optionsWithCurrentId)
                    return;

                const selectedElement = optionsWithCurrentId[0];

                $scope.lifeCycleStatusModel = {
                    selectedElement: selectedElement,
                    select2Config: select2LoadingService.select2LocalDataNoSearch(() => options, false),
                    elementSelected: (newElement) => {
                        $scope.usage.lifeCycleStatus = newElement.optionalObjectContext;
                        const payload = { lifeCycleStatus: newElement.optionalObjectContext };
                        patch(payload, saveUrlWithOrgId);
                    }
                }
            }
        }
    ]);
})(angular, app);
