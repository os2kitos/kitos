module Kitos.GlobalConfig.Directives {
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

        public mainGrid: IKendoGrid<Models.IRoleEntity>;
        public mainGridOptions: IKendoGridOptions<Models.IRoleEntity>;

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
                            url: `${this.optionsUrl}?$filter=IsActive eq true`,
                            dataType: "json"
                        },
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
                        template: "<a ng-click='ctrl.opretRolle()' class='btn btn-success pull-right'>#: text #</a>"
                    }
                ],
                pageable: {
                    refresh: true,
                    pageSizes: [10, 25, 50, 100, 200],
                    buttonCount: 5
                },
                sortable: {
                    mode: "single",
                    
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
                            { text: "Op/Ned", click: this.changePriority, imageClass: "k-edit", className: "k-custom-edit", iconClass: "k-icon" } /* kendo typedef is missing imageClass and iconClass so casting to any */ as any,
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
                    },
                ]
            };
        
            /* var initMainGrid = this.$('#mainGrid').data('kendoGrid');

            
           // console.log(this.mainGrid.table);

           initMainGrid.table.kendoSortable({
                filter: ">tbody >tr",
                hint: $.noop,
                cursor: "move",
                placeholder: function (element) {
                    return element.clone().addClass("k-state-hover").css("opacity", 0.65);
                },
                container: "#grid tbody",
                change: function (e) {
                    var skip = this.mainGrid.dataSource.skip(),
                        oldIndex = e.oldIndex + skip,
                        newIndex = e.newIndex + skip,
                        data = this.mainGrid.dataSource.data(),
                        dataItem = this.mainGrid.dataSource.getByUid(e.item.data("uid"));

                    this.mainGrid.dataSource.remove(dataItem);
                    this.mainGrid.dataSource.insert(newIndex, dataItem);

                }
            });*/




         /*  this.mainGrid = {
                getOptions() { return this.mainGridOptions },
                columns: this.mainGridOptions.columns,
                dataSource: this.mainGridOptions.dataSource,
                options: this.mainGridOptions,
                tbody: null,
                pager: null,
                footer: null,
                table: {kendoSortable:null,},
                thead: null,
                content: null,
                lockedHeader: null,
                lockedTable: null,
                lockedContent: null,
                element: null,
                wrapper: null,
                addRow() { return null; },
                autoFitColumn() { return null },
                cancelChanges() { return null },
                cancelRow() { return null },
                cellIndex() { return null },
                clearSelection() { return null },
                bind() { return null },
                closeCell() { return null },
                collapseGroup() { return null },
                collapseRow() { return null },
                current() { return null },
                dataItem() { return null },
                destroy() { return null },
                editCell() { return null },
                editRow() { return null },
                events: null,
                expandGroup() { return null },
                expandRow() { return null },
                unlockColumn() { return null },
                first() { return null },
                hideColumn() { return null },
                init() { return null },
                items() { return null },
                lockColumn() { return null },
                one() { return null },
                refresh() { return null },
                removeRow() { return null },
                reorderColumn() { return null },
                resize() { return null },
                saveAsExcel() { return null },
                saveAsPDF() { return null },
                saveChanges() { return null },
                saveRow() { return null },
                select() { return null },
                setDataSource() { return null },
                setOptions() { return null },
                showColumn() { return null },
                trigger() { return null },
                unbind() { return null }
            };
            
           
           this.mainGrid = {
                getOptions() { return this.mainGridOptions},
                columns: this.mainGridOptions.columns,
                dataSource: this.mainGridOptions.dataSource,
                options: this.mainGridOptions,
                footer: null,
                pager: null,
                table: {
                    kendoSortable() {



                    },
                },
                tbody: null,
                thead: null,
                content: null,
                lockedHeader: null,
                lockedTable: null,
                lockedContent: null,
                element: null,
                wrapper: null,
                addRow() { return null; },
                autoFitColumn() { return null },
                cancelChanges() { return null },
                cancelRow() { return null },
                cellIndex() { return null },
                clearSelection() { return null },
                bind() { return null },
                closeCell() { return null },
                collapseGroup() { return null },
                collapseRow() { return null },
                current() { return null },
                dataItem() { return null },
                destroy() { return null },
                editCell() { return null },
                editRow() { return null },
                events: null,
                expandGroup() { return null },
                expandRow() { return null },
                unlockColumn() { return null },
                first() { return null },
                hideColumn() { return null },
                init() { return null },
                items() { return null },
                lockColumn() { return null },
                one() { return null },
                refresh() { return null },
                removeRow() { return null },
                reorderColumn() { return null },
                resize() { return null },
                saveAsExcel() { return null },
                saveAsPDF() { return null },
                saveChanges() { return null },
                saveRow() { return null },
                select() { return null },
                setDataSource() { return null },
                setOptions() { return null },
                showColumn() { return null },
                trigger() { return null },
                unbind() { return null }
           }
           
           this.mainGrid.table.kendoSortable({
               filter: ">tbody >tr",
               hint: $.noop,
               cursor: "move",
               placeholder: function (element) {
                   return element.clone().addClass("k-state-hover").css("opacity", 0.65);
               },
               container: "#grid tbody",
               change: function (e) {
                   var skip = this.mainGrid.dataSource.skip(),
                       oldIndex = e.oldIndex + skip,
                       newIndex = e.newIndex + skip,
                       data = this.mainGrid.dataSource.data(),
                       dataItem = this.mainGrid.dataSource.getByUid(e.item.data("uid"));

                   this.mainGrid.dataSource.remove(dataItem);
                   this.mainGrid.dataSource.insert(newIndex, dataItem);

               }
           }); */

           /* this.mainGrid = {
                table: {
                    kendoSortable({
                        filter: ">tbody >tr",
                        hint: $.noop,
                        cursor: "move",
                        placeholder: function (element) {
                            return element.clone().addClass("k-state-hover").css("opacity", 0.65);
                        },
                        container: "#grid tbody",
                        change: function (e) {
                            var skip = this.mainGrid.dataSource.skip(),
                                oldIndex = e.oldIndex + skip,
                                newIndex = e.newIndex + skip,
                                data = this.mainGrid.dataSource.data(),
                                dataItem = this.mainGrid.dataSource.getByUid(e.item.data("uid"));

                            this.mainGrid.dataSource.remove(dataItem);
                            this.mainGrid.dataSource.insert(newIndex, dataItem);

                        }
                    };
                   
                    
                }

            };*/
        }

        private onEdit = (e: JQueryEventObject) => {
            e.preventDefault();
           // var dataItem = this.mainGrid.dataItem(this.$(e.currentTarget).closest("tr"));
           // var entityId = dataItem["Id"];
           // this.$state.go("organization.user.edit", { id: entityId });
        }
       
        private changePriority = (e: JQueryEventObject) => {
            alert("click");
            var test = this.mainGrid.table;
            e.preventDefault();
            //var dataItem = this.mainGrid.dataItem(this.$(e.currentTarget).closest("tr"));
            //var entityId = dataItem["Id"];
            //this.$state.go("organization.user.edit", { id: entityId });
        }
}
    angular.module("app")
        .directive("globalOptionRoleList", setupDirective);

    
}