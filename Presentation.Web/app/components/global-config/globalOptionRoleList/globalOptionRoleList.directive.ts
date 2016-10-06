﻿module Kitos.GlobalConfig.Directives {
    "use strict";

    function setupDirective(): ng.IDirective {
        return {
            scope: {},
            controller: GlobalOptionRoleListDirective,
            controllerAs: "ctrl",
            bindToController: {
                optionsUrl: "@",
                title: "@"
            },
            template: `<h2>{{ ctrl.title }}</h2><div id="mainGrid" data-kendo-grid="{{ ctrl.mainGrid }}" data-k-options="{{ ctrl.mainGridOptions }}"></div>`
        };
    }

    interface IDirectiveScope {
        optionsUrl: string;
        title: string;
    }

    class GlobalOptionRoleListDirective implements IDirectiveScope {
        public optionsUrl: string;
        public title: string;

        //public mainGrid: IKendoGrid<Models.IRoleEntity>;
        public mainGridOptions: IKendoGridOptions<Models.IRoleEntity>;

        public static $inject: string[] = ["$http", "$timeout", "_", "$", "$state", "notify"];

        constructor(
            private $http: ng.IHttpService,
            private $timeout: ng.ITimeoutService,
            private _: ILoDashWithMixins,
            private $: JQueryStatic,
            //private $state: ng.ui.IStateService,
            private notify) {
            this.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: `${this.optionsUrl}?$filter=IsActive eq true`,
                            dataType: "json"
                        }
                        //,destroy: {
                        //    url: (entity) => {
                        //        return `/odata/Organizations(${this.user.currentOrganizationId})/RemoveUser()`;
                        //    },
                        //    dataType: "json",
                        //    contentType: "application/json"
                        //},
                    },
                    sort: {
                        field: "Name",
                        dir: "asc"
                    },
                    pageSize: 100,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true,
                    schema: {
                        model: {
                            id: "Id"
                        }
                    }
                } as kendo.data.DataSourceOptions,
                toolbar: [
                    {
                        //TODO ng-show='hasWriteAccess'
                        name: "opretRolle",
                        text: "Opret rolle",
                        template: "<a ng-click='ctrl.createOption()' class='btn btn-success pull-right'>#: text #</a>"
                    }
                ],
                pageable: {
                    refresh: true,
                    pageSizes: [10, 25, 50, 100, 200],
                    buttonCount: 5
                },
                sortable: {
                    mode: "single"
                },
                editable: "popup",
                reorderable: true,
                resizable: true,
                filterable: {
                    mode: "row"
                },
                groupable: false,
                columnMenu: {
                    filterable: false
                },
                columns: [
                    {
                        field: "IsActive", title: "Aktiv", width: 112,
                        persistId: "isActive", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: `# if(IsActive) { # <span class="glyphicon glyphicon-check text-success" aria-hidden="true"></span> # } else { # <span class="glyphicon glyphicon-unchecked" aria-hidden="true"></span> # } #`,
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        command: [
                            { text: "Op/Ned", click: this.editOption, imageClass: "k-edit", className: "k-custom-edit", iconClass: "k-icon" } /* kendo typedef is missing imageClass and iconClass so casting to any */ as any,
                        ],
                        title: " ", width: 176,
                        persistId: "command"
                    },
                    {
                        field: "Id", title: "Nr.", width: 230,
                        persistId: "id", // DON'T YOU DARE RENAME!
                        template: (dataItem) => dataItem.Id.toString(),
                        hidden: false,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "Name", title: "Navn", width: 230,
                        persistId: "name", // DON'T YOU DARE RENAME!
                        template: (dataItem) => dataItem.Name,
                        hidden: false,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        field: "HasWriteAccess", title: "Skriv", width: 112,
                        persistId: "hasWriteAccess", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: `# if(HasWriteAccess) { # <span class="glyphicon glyphicon-check text-success" aria-hidden="true"></span> # } else { # <span class="glyphicon glyphicon-unchecked" aria-hidden="true"></span> # } #`,
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        //field: "Note", title: "Beskrivelse", width: 230,
                        field: "Description", title: "Beskrivelse", width: 230,
                        persistId: "note", // DON'T YOU DARE RENAME!
                        template: (dataItem) => dataItem.Note,
                        hidden: false,
                        filterable: {
                            cell: {
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        command: [
                            { text: "Redigér", click: this.editOption, imageClass: "k-edit", className: "k-custom-edit", iconClass: "k-icon" } /* kendo typedef is missing imageClass and iconClass so casting to any */ as any,
                           ],
                        title: " ", width: 176,
                        persistId: "command"
                    }
                ]
            };
        }

        public createOption = () => {
            //TODO OS2KITOS-256
        }

        private editOption = (e: JQueryEventObject) => {
            //TODO OS2KITOS-257
            e.preventDefault();
            //var dataItem = this.mainGrid.dataItem(this.$(e.currentTarget).closest("tr"));
            //var entityId = dataItem["Id"];
            //this.$state.go("organization.user.edit", { id: entityId });
        }

        public deactivateOption = () => {
            //TODO OS2KITOS-258
        }
    }
    angular.module("app")
        .directive("globalOptionRoleList", setupDirective);
}