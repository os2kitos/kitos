((ng, app) => {

    app.controller("object.EditAdviceCtrl",
        [
            "$", "$scope", "$http", "notify", "$uibModal", "object", "type", "advicename", "hasWriteAccess", "localOptionServiceFactory", "currentUser",
            ($, $scope, $http, notify, $modal, object, type: Kitos.Models.Advice.AdviceType, advicename, hasWriteAccess,
                localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory, currentUser: Kitos.Services.IUser) => {
                $scope.type = type;
                $scope.object = object;
                $scope.advicename = advicename;
                $scope.hasWriteAccess = hasWriteAccess;

                const localRoleType: Kitos.Services.LocalOptions.LocalOptionType = Kitos.Services.LocalOptions.getLocalOptionTypeFromAdvisType(type);
                const roleProperty = Kitos.Models.Advice.getAdviceTypeUserRelationRoleProperty(type);

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
                                    url: `/Odata/GetAdvicesByOrganizationId(organizationId=${currentUser.currentOrganizationId})?$filter=type eq '${type}' and RelationId eq ${object.id
                                        }&$expand=Reciepients($expand=${roleProperty}), Advicesent`,
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
                        change: onChange,
                        columns: [
                            {
                                field: "IsActive",
                                title: "Aktiv",
                                template: data => data.IsActive ? "<i class='fa fa-check'></i> " : "",
                                width: 45,
                                attributes: { "class": "centeredContent" }
                            },
                            {
                                field: "Name",
                                title: "Navn",
                                template: data => {
                                    const name = data.Name || "Ikke navngivet";
                                    return name;
                                },
                                attributes: { "class": "might-overflow" }
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
                                attributes: { "class": "might-overflow" }
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
                                attributes: { "class": "might-overflow" }
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
                                attributes: { "class": "might-overflow" }
                            },
                            {
                                field: "Reciepients",
                                title: "Modtager",
                                template: (dataItem) => Kitos.Models.ViewModel.Advice.renderReceivers(dataItem, type, "RECIEVER", localOptionLookup),
                                attributes: { "class": "might-overflow" },
                                sortable: false //Not possible on collection field
                            },
                            {
                                field: "Reciepients",
                                title: "CC",
                                template: (dataItem) => Kitos.Models.ViewModel.Advice.renderReceivers(dataItem, type, "CC", localOptionLookup),
                                attributes: { "class": "might-overflow" },
                                sortable: false //Not possible on collection field
                            },
                            {
                                field: "Subject",
                                title: "Emne"
                            },
                            {
                                template: (dataItem) => {
                                    if (hasWriteAccess) {
                                        return `<button class="btn-link" data-ng-click="newAdvice('PATCH',${dataItem.Id})" data-element-type="editAdviceButton">
                                    <i class="glyphicon glyphicon-pencil"></i></button>
                                    <button class="btn-link" ng-if="${dataItem.CanBeDeleted}" data-confirm-click="Er du sikker på at du vil slette?" data-confirmed-click="deleteAdvice(${dataItem.Id})" data-element-type="deleteAdviceButton"><i class="glyphicon glyphicon-trash"></i></button>`;
                                    } else {
                                        return "Ingen rettigheder";
                                    }
                                }
                            }
                        ],
                        toolbar: [
                            {
                                name: "advis",
                                text: "Opret advis",
                                template:
                                    "<button id=\"NewAdviceButton\" data-ng-disabled=\"!hasWriteAccess\" class=\"btn btn-success btn-sm\" data-ng-click=\"newAdvice('POST')\"><i class=\"glyphicon glyphicon-plus small\" ></i>Ny</button>"
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
                    };

                    $scope.detailGridOptions = {
                        dataSource: {
                            type: "odata-v4",
                            transport: {
                                read: {
                                    url: "/Odata/adviceSent?$filter=AdviceId eq 0",
                                    dataType: "json"
                                },
                            },
                            pageSize: 25
                        },
                        columns: [
                            {
                                field: "AdviceSentDate",
                                title: "Afsendt",
                                template: x => {
                                    if (x.AdviceSentDate != null) {
                                        return kendo.toString(new Date(x.AdviceSentDate), "g");
                                    }
                                    return "";
                                }
                            }
                        ],
                        scrollable: {
                            virtual: true
                        }
                    };

                    $scope.datepickerOptions = Kitos.Configs.standardKendoDatePickerOptions;

                    $scope.deleteAdvice = (id) => {
                        $http.delete(`odata/advice(${id})`)
                            .then(() => {
                                notify.addSuccessMessage("Advisen er slettet!");
                                $("#mainGrid").data("kendoGrid").dataSource.read();
                            },
                                () => notify.addErrorMessage("Fejl! Kunne ikke slette!"));
                    };

                    $scope.newAdvice = (action, id) => {
                        $scope.hasWriteAccess = hasWriteAccess;
                        $scope.action = action;
                        var modalInstance = Kitos.ItAdvice.Modal.Create.createModalInstance(_, $, $modal, $scope, notify, $http, type, action, id, hasWriteAccess);
                        modalInstance.result.then(angular.noop, angular.noop);
                    };

                    function onChange(arg) {
                        const grid = $("#mainGrid").data("kendoGrid");
                        const selectedItem = grid.dataItem(grid.select());
                        $("#detailGrid").data("kendoGrid").dataSource.transport.options.read.url =
                            `/Odata/adviceSent?$filter=AdviceId eq ${selectedItem.Id}`;
                        $("#detailGrid").data("kendoGrid").dataSource.read();
                    };
                });
            }
        ]);
})(angular, app);