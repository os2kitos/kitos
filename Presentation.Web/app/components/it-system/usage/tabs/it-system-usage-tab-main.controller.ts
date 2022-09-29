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
                ]
            }
        });
    }]);

    app.controller("system.EditMain", ["$rootScope", "$scope", "$http", "notify", "user", "systemCategories",
        "autofocus", "itSystemUsageService", "select2LoadingService", "uiState",
        ($rootScope, $scope, $http, notify, user, systemCategories, autofocus,
            itSystemUsageService: Kitos.Services.ItSystemUsage.IItSystemUsageService,
            select2LoadingService: Kitos.Services.ISelect2LoadingService,
            uiState: Kitos.Models.UICustomization.ICustomizedModuleUI) => {
            var itSystemUsage = new Kitos.Models.ViewModel.ItSystemUsage.SystemUsageViewModel($scope.usage);
            const blueprint = Kitos.Models.UICustomization.Configs.BluePrints.ItSystemUsageUiCustomizationBluePrint;

            $rootScope.page.title = "IT System - Anvendelse";
            $scope.autoSaveUrl = `api/itSystemUsage/${itSystemUsage.id}`;
            $scope.hasViewAccess = user.currentOrganizationId === itSystemUsage.organizationId;
            $scope.systemCategories = systemCategories;
            $scope.shouldShowCategories = systemCategories.length > 0;
            $scope.system = itSystemUsage.itSystem;
            $scope.lastChangedBy = itSystemUsage.lastChangedBy;
            $scope.lastChanged = itSystemUsage.lastChanged;
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

            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            bindLifeCycleStatusModel();
            reloadValidationStatus();

            $scope.patchDate = (field, value) => {
                var expirationDate = itSystemUsage.expirationDate;
                var concluded = itSystemUsage.concluded;
                var formatDateString = "YYYY-MM-DD";
                var fromDate = moment(concluded, [Kitos.Constants.DateFormat.DanishDateFormat, formatDateString]).startOf("day");
                var endDate = moment(expirationDate, [Kitos.Constants.DateFormat.DanishDateFormat, formatDateString]).endOf("day");
                var date = moment(value, Kitos.Constants.DateFormat.DanishDateFormat);
                
                if (value === "" || value == undefined) {
                    var payload = {};
                    payload[field] = null;
                    patch(payload, saveUrlWithOrgId);
                } else if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig.");
                }
                else if (fromDate != null && endDate != null && fromDate >= endDate) {
                    notify.addErrorMessage("Den indtastede slutdato er før startdatoen.");
                }
                else {
                    const dateString = date.format(formatDateString);
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
                itSystemUsageService.getValidationDetails(itSystemUsage.id).then(newStatus => {
                    $scope.validationStatus = newStatus;
                });
            }

            function bindLifeCycleStatusModel() {
                const lifeCycleStatusOptions = new Kitos.Models.ItSystemUsage.LifeCycleStatusOptions();
                const options = lifeCycleStatusOptions.options;
                const currentId = itSystemUsage.lifeCycleStatus ?? 0;
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
