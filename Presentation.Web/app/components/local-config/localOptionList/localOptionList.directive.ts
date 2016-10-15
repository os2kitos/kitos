module Kitos.LocalAdmin.Directives {
    "use strict";

    function setupDirective(): ng.IDirective {
        return {
            scope: {},
            controller: LocalOptionListDirective,
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

    class LocalOptionListDirective implements IDirectiveScope {
        public optionsUrl: string;
        public title: string;

        public mainGrid: IKendoGrid<Models.IOptionEntity>;
        public mainGridOptions: IKendoGridOptions<Models.IOptionEntity>;

        public static $inject: string[] = ["$http", "$timeout", "_", "$", "$state", "notify"];

        constructor(
            private $http: ng.IHttpService,
            private $timeout: ng.ITimeoutService,
            private _: ILoDashWithMixins,
            private $: JQueryStatic,
            private $state: ng.ui.IStateService,
            private notify) {
            this.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: this.optionsUrl,
                            dataType: "json"
                        }
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
                        template: `<input type="checkbox" data-ng-model="dataItem.IsActive" data-global-option-id="{{ dataItem.Id }}" data-autosave="${ this.optionsUrl }" data-field="OptionId"> {{ Name }}`,
                        hidden: false,
                        filterable: false,
                        sortable: false
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
                        field: "Description", title: "Beskrivelse", width: 230,
                        persistId: "description", // DON'T YOU DARE RENAME!
                        template: (dataItem) => dataItem.Description,
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
                            { text: "Redigér", click: this.onEdit, imageClass: "k-edit", className: "k-custom-edit", iconClass: "k-icon" } /* kendo typedef is missing imageClass and iconClass so casting to any */ as any,
                        ],
                        title: " ", width: 176,
                        persistId: "command"
                    }
                ]
            };
        }

        private onEdit = (e: JQueryEventObject) => {
            e.preventDefault();
            //var dataItem = this.mainGrid.dataItem(this.$(e.currentTarget).closest("tr"));
            //var entityId = dataItem["Id"];
            //this.$state.go("organization.user.edit", { id: entityId });
        }

        public refreshKendoGrid() {
            console.log(this.mainGrid);
            console.log(123);
            if (this.mainGrid) {
                console.log(1234);
                this.mainGrid.refresh();
            }
        }
    }
    angular.module("app")
        .directive("localOptionList", setupDirective);
}