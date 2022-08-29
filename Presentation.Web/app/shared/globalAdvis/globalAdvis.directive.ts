((ng, app) => {
    app.directive("globalAdvis", [
        () => ({
            templateUrl: "app/shared/globalAdvis/it-advice-global.view.html",
            scope: {
                stateName: "@",
                user: "="
            },
            controller: [
                "$scope",
                "$uibModal",
                "$window",
                "localOptionServiceFactory",
                (
                    $scope,
                    $uibModal,
                    $window,
                    localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) => {

                    const stateNames = {
                        itSystemUsages: Kitos.Constants.SRef.SystemUsageOverview,
                        itContracts: Kitos.Constants.SRef.ContractOverview,
                        dataProcessingRegistrations: Kitos.Constants.SRef.DataProcessingRegistrationOverview
                    };

                    $scope.$watch("stateName", (newValue, oldValue) => {
                        if ($scope.stateName === stateNames.dataProcessingRegistrations || $scope.stateName === stateNames.itContracts || $scope.stateName === stateNames.itSystemUsages)
                            $scope.disableAdvisLink = false;
                        else
                            $scope.disableAdvisLink = true;
                    });
                    var parent = $scope;
                    $scope.showAdviceModal = () => {
                        $uibModal.open({
                            windowClass: "modal fade in",
                            templateUrl: "app/shared/globalAdvis/it-advice-global.modal.view.html",
                            size: "lg",
                            controller: ["$scope", "$uibModalInstance", "notify", ($scope, $modalInstance, nofity) => {
                                var stateUrl = "";
                                var moduleTypeFilter = "";
                                let adviceType: Kitos.Models.Advice.AdviceType | null = null;
                                if (parent.stateName === stateNames.itContracts) {
                                    $scope.title = "IT advis - IT Kontrakter";
                                    moduleTypeFilter = "Type eq 'itContract'";
                                    stateUrl = $window.location.href.replace("overview", "edit");
                                    adviceType = Kitos.Models.Advice.AdviceType.ItContract;
                                }
                                if (parent.stateName === stateNames.itSystemUsages) {
                                    $scope.title = "IT advis - IT Systemer";
                                    moduleTypeFilter = "Type eq 'itSystemUsage'";
                                    stateUrl = $window.location.href.replace("overview", "usage");
                                    adviceType = Kitos.Models.Advice.AdviceType.ItSystemUsage;
                                }
                                if (parent.stateName === stateNames.dataProcessingRegistrations) {
                                    $scope.title = "IT advis - Databehandleraftaler";
                                    moduleTypeFilter = "Type eq 'dataProcessingRegistration'";
                                    stateUrl = $window.location.href.replace("overview", "edit");
                                    adviceType = Kitos.Models.Advice.AdviceType.DataProcessingRegistration;
                                }
                                if (adviceType === null) {
                                    throw `Unable to map advice type based on state:${parent.stateName}`;
                                }
                                const localRoleType: Kitos.Services.LocalOptions.LocalOptionType = Kitos.Services.LocalOptions.getLocalOptionTypeFromAdvisType(adviceType);
                                const roleProperty = Kitos.Models.Advice.getAdviceTypeUserRelationRoleProperty(adviceType);

                                const localOptionsService = localOptionServiceFactory.create(localRoleType);
                                localOptionsService.getAll().then(options => {
                                    const localOptionLookup = options.reduce<{ [key: number]: Kitos.Models.IOptionEntity }>(
                                        (acc, option) => {
                                            acc[option.Id] = option;
                                            return acc;
                                        },
                                        {});

                                    $scope.mainGridOptions = {
                                        dataSource: {
                                            type: "odata-v4",
                                            transport: {
                                                read: {
                                                    url: `/Odata/GetAdvicesByOrganizationId(organizationId=${$scope.user.currentOrganizationId})?$filter=${moduleTypeFilter} AND IsActive eq true&$expand=Reciepients($expand=${roleProperty}), Advicesent`,
                                                    dataType: "json"
                                                },
                                            },
                                            sort: {
                                                field: "AlarmDate",
                                                dir: "asc"
                                            },
                                            pageSize: 10,
                                            serverPaging: true,
                                            serverFiltering: true,
                                            serverSorting: true
                                        },
                                        selectable: true,
                                        columns: [{
                                            field: "Name",
                                            title: "Navn",
                                            template: data => {
                                                const name = data.Name || "Ikke navngivet";
                                                return `<a ng-click="$dismiss()" href="${stateUrl}/${data.RelationId}/advice">${name}</a>`;
                                            },
                                            attributes: { "class": "might-overflow" },
                                            sortable: true
                                        },
                                        {
                                            field: "SentDate",
                                            title: "Sidst sendt",
                                            template: x => {
                                                if (x.SentDate != null) {
                                                    return kendo.toString(new Date(x.SentDate), "d");
                                                }
                                                return "";
                                            },
                                            attributes: { "class": "might-overflow" },
                                            sortable: true
                                        },
                                        {
                                            field: "Id",
                                            hidden: true
                                        },

                                        {
                                            field: "AlarmDate",
                                            title: "Fra dato",
                                            template: x => {
                                                if (x.AlarmDate != null) {
                                                    return kendo.toString(new Date(x.AlarmDate), "d");
                                                }
                                                return "";
                                            },
                                            attributes: { "class": "might-overflow" },
                                            sortable: true
                                        },
                                        {
                                            field: "StopDate",
                                            title: "Til dato",
                                            template: x => {
                                                if (x.StopDate != null) {
                                                    return kendo.toString(new Date(x.StopDate), "d");
                                                }
                                                return "";
                                            },
                                            attributes: { "class": "might-overflow" },
                                            sortable: true
                                        },
                                        {
                                            field: "Reciepients.Email", title: "Modtager",
                                            template: (dataItem) => Kitos.Models.ViewModel.Advice.renderReceivers(dataItem, adviceType, "RECIEVER", localOptionLookup),
                                            attributes: { "class": "might-overflow" },
                                            sortable: false //Not possible on collection field
                                        },
                                        {
                                            field: "Reciepients.Email",
                                            title: "CC",
                                            template: (dataItem) => Kitos.Models.ViewModel.Advice.renderReceivers(dataItem, adviceType, "CC", localOptionLookup),
                                            attributes: { "class": "might-overflow" },
                                            sortable: false //Not possible on collection field
                                        },
                                        {
                                            field: "Subject",
                                            title: "Emne",
                                            sortable: true
                                        }
                                        ],
                                        sortable: {
                                            mode: "single"
                                        },
                                        pageable: {
                                            refresh: true,
                                            pageSizes: [10, 25, 50, 100, 200],
                                            buttonCount: 5
                                        },
                                    }
                                });
                            }]
                        });
                    }
                }]
        })
    ]);
})(angular, app);
