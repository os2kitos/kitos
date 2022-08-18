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
                (
                    $scope,
                    $uibModal,
                    $window) => {
                    $scope.$watch("stateName", (newValue, oldValue) => {
                        if ($scope.stateName === "it-project.overview" || $scope.stateName === "it-system.overview" || $scope.stateName === "it-contract.overview" || $scope.stateName === "it-contract.plan" || $scope.stateName === "data-processing.overview")
                            $scope.disableAdvisLink = false;
                        else
                            $scope.disableAdvisLink = true;
                    });
                    var parent = $scope;
                    //TODO: Also need the local role names and a lookup for this to work.. also with fallback to "ikke anvendt"
                    $scope.showAdviceModal = () => {
                        $uibModal.open({
                            windowClass: "modal fade in",
                            templateUrl: "app/shared/globalAdvis/it-advice-global.modal.view.html",
                            size: "lg",
                            controller: ["$scope", "$uibModalInstance", "notify", ($scope, $modalInstance, nofity) => {
                                var stateUrl = "";
                                var moduleTypeFilter = "";
                                let adviceType: Kitos.Models.Advice.AdviceType | null = null;
                                if (parent.stateName === "it-project.overview") {
                                    $scope.title = "IT advis - IT Projekter";
                                    moduleTypeFilter = "Type eq 'itProject'";
                                    stateUrl = $window.location.href.replace("overview", "edit");
                                    adviceType = Kitos.Models.Advice.AdviceType.ItProject;
                                }
                                if (parent.stateName === "it-contract.overview" || parent.stateName === "it-contract.plan") {
                                    $scope.title = "IT advis - IT Kontrakter";
                                    moduleTypeFilter = "Type eq 'itContract'";
                                    stateUrl = $window.location.href.replace("overview", "edit");
                                    adviceType = Kitos.Models.Advice.AdviceType.ItContract;
                                }
                                if (parent.stateName === "it-system.overview") {
                                    $scope.title = "IT advis - IT Systemer";
                                    moduleTypeFilter = "Type eq 'itSystemUsage'";
                                    stateUrl = $window.location.href.replace("overview", "usage");
                                    adviceType = Kitos.Models.Advice.AdviceType.ItSystemUsage;
                                }
                                if (parent.stateName === "data-processing.overview") {
                                    $scope.title = "IT advis - Databehandleraftaler";
                                    moduleTypeFilter = "Type eq 'dataProcessingRegistration'";
                                    stateUrl = $window.location.href.replace("overview", "edit");
                                    adviceType = Kitos.Models.Advice.AdviceType.DataProcessingRegistration;
                                }
                                if (adviceType === null) {
                                    throw `Unable to map advice type based on state:${parent.stateName}`;
                                }
                                const roleProperty = Kitos.Models.Advice.getAdviceTypeUserRelationRoleProperty(adviceType);
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
                                        template: (dataItem) => Kitos.Models.ViewModel.Advice.renderReceivers(dataItem, adviceType, "RECIEVER"),
                                        attributes: { "class": "might-overflow" },
                                        sortable: false //Not possible on collection field
                                    },
                                    {
                                        field: "Reciepients.Email",
                                        title: "CC",
                                        template: (dataItem) => Kitos.Models.ViewModel.Advice.renderReceivers(dataItem, adviceType, "CC"),
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
                            }]
                        });
                    }
                }]
        })
    ]);
})(angular, app);
