module Kitos.GlobalConfig.Directives {
    "use strict";

    function setupDirective(): ng.IDirective {
        return {
            scope: {
                editState: "@state",
                dirId: "@",
                optionType: "@"
            },
            controller: GlobalOptionRoleListDirective,
            controllerAs: "ctrl",
            bindToController: {
                title: "@",
                optionsUrl: "@"
            },
            template: `<h2>{{ ctrl.title }}</h2><div id="{{ctrl.dirId}}" data-kendo-grid="{{ ctrl.mainGrid }}" data-k-options="{{ ctrl.mainGridOptions }}"></div>`
        };
    }

    interface IDirectiveScope {
        title: string;
        editState: string;
        optionsUrl: string;
        optionId: string;
        optionType: string;
    }

    class GlobalOptionRoleListDirective implements IDirectiveScope {
        public optionsUrl: string;
        public title: string;
        public editState: string;
        public optionId: string;
        public dirId: string;
        public optionType: string;
        public mainGrid: IKendoGrid<Models.IRoleEntity>;
        public mainGridOptions: IKendoGridOptions<Models.IRoleEntity>;

        public static $inject: string[] = ["$http", "$timeout", "_", "$", "$state", "notify", "$scope"];

        constructor(
            private $http: ng.IHttpService,
            private $timeout: ng.ITimeoutService,
            private _: ILoDashWithMixins,
            private $: JQueryStatic,
            private $state: ng.ui.IStateService,
            private notify,
            private $scope) {

            this.$scope.$state = $state;
            this.editState = $scope.editState;
            this.dirId = $scope.dirId;
            this.optionType = $scope.optionType;

            this.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: `${this.optionsUrl}`,
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
                        field: "priority",
                        dir: "desc"
                    },
                    pageSize: 100,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true,
                    schema: {
                        model: {
                            id: "Id",
                            fields: {
                                IsObligatory: { type: "boolean" },
                                Name: { type: "string" },
                                HasWriteAccess: { type: "boolean" },
                                Description: { type: "string" }
                            }
                        }
                    }
                } as kendo.data.DataSourceOptions,
                toolbar: [
                    {
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
                        field: "IsEnabled",
                        title: "Tilgængelig",
                        width: 112,
                        persistId: "isEnabled", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: `# if(IsEnabled) { # <span class="glyphicon glyphicon-check text-success" aria-hidden="true"></span> # } else { # <span class="glyphicon glyphicon-unchecked" aria-hidden="true"></span> # } #`,
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "IsObligatory",
                        title: "Obligatorisk",
                        width: 112,
                        persistId: "IsObligatory", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: `# if(IsObligatory) { # <span class="glyphicon glyphicon-check text-success" aria-hidden="true"></span> # } else { # <span class="glyphicon glyphicon-unchecked" aria-hidden="true"></span> # } #`,
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        title: "Prioritet",
                        template: `<button class='btn btn-link' data-ng-click='ctrl.pushUp($event)'"><i class='fa fa-arrow-up' aria-hidden='true'></i></button>` +
                        `<button class='btn btn-link' data-ng-click='ctrl.pushDown($event)'"><i class='fa fa-arrow-down' aria-hidden='true'></i></button>`,
                        width: 100,
                        attributes: { "class": "text-center" },
                        persistId: "command"

                    },
                    {
                        field: "Name",
                        title: "Navn",
                        width: 230,
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
                        field: "HasWriteAccess",
                        title: "Skriv",
                        width: 112,
                        persistId: "hasWriteAccess", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template:
                        `# if(HasWriteAccess) { # <span class="glyphicon glyphicon-check text-success" aria-hidden="true"></span> # } else { # <span class="glyphicon glyphicon-unchecked" aria-hidden="true"></span> # } #`,
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "Description",
                        title: "Beskrivelse",
                        width: 230,
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
                        name: "editOption",
                        text: "Redigér",
                        template: "<button type='button' class='btn btn-link' title='Redigér rolle' ng-click='ctrl.editOption($event)'><i class='fa fa-pencil' aria-hidden='true'></i></button> <button type='button' class='btn btn-link' title='Gør rolle utilgængelig' ng-click='ctrl.disableEnableOption($event, false)' ng-if='dataItem.IsEnabled'><i class='fa fa-times' aria-hidden='true'></i></button> <button type='button' class='btn btn-link' title='Gør rolle tilgængelig' ng-click='ctrl.disableEnableOption($event, true)' ng-if='!dataItem.IsEnabled'><i class='fa fa-check' aria-hidden='true'></i></button>",
                        title: " ",
                        width: 176
                    } as any
                ]
            };
        }

        public createOption = () => {
            this.$scope.$state.go(this.editState, { id: 0, optionsUrl: this.optionsUrl, optionType: this.optionType });
        };

        public editOption = (e: JQueryEventObject) => {
            e.preventDefault();
            var entityGrid = this.$(`#${this.dirId}`).data("kendoGrid");
            var selectedItem = entityGrid.dataItem(this.$(e.currentTarget).closest("tr"));
            this.optionId = selectedItem.get("id");
            this.$scope.$state.go(this.editState, { id: this.optionId, optionsUrl: this.optionsUrl, optionType: this.optionType });
        };

        public disableEnableOption = (e: JQueryEventObject, enable: boolean) => {
            e.preventDefault();
            var superClass = this;
            var entityGrid = this.$(`#${this.dirId}`).data("kendoGrid");
            var selectedItem = entityGrid.dataItem(this.$(e.currentTarget).closest("tr"));
            var entityId = selectedItem.get("Id");
            var payload = { IsEnabled: enable };

            var msg = this.notify.addInfoMessage("Gemmer...", false);

            this.$http.patch(this.optionsUrl + "(" + entityId + ")", payload)
                .success(function () {
                    msg.toSuccessMessage("Feltet er opdateret.");
                    superClass.$(`#${superClass.dirId}`).data("kendoGrid").dataSource.read();
                })
                .error(function (result, status) {
                    if (status === 409) {
                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres da værdien den allerede findes i KITOS!");
                    } else {
                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                    }
                });
        };

        private pushUp = (e: JQueryEventObject) => {
            e.preventDefault();

            var entityGrid = this.$(`#${this.dirId}`).data("kendoGrid");
            var selectedItem = entityGrid.dataItem(this.$(e.currentTarget).closest("tr"));
            var priority: number = selectedItem.get("Priority");

            this.optionId = selectedItem.get("Id");

            let payload = {
                Priority: priority + 1
            }

            this.$http.patch(`${this.optionsUrl}(${this.optionId})`, payload).then((response) => {
                this.$(`#${this.dirId}`).data("kendoGrid").dataSource.read();
            });

        };

        private pushDown = (e: JQueryEventObject) => {
            e.preventDefault();

            var entityGrid = this.$(`#${this.dirId}`).data("kendoGrid");
            var selectedItem = entityGrid.dataItem(this.$(e.currentTarget).closest("tr"));
            var priority: number = selectedItem.get("Priority");

            this.optionId = selectedItem.get("Id");

            let payload = {
                Priority: priority - 1
            }
            this.$http.patch(`${this.optionsUrl}(${this.optionId})`, payload).then((response) => {
                this.$(`#${this.dirId}`).data("kendoGrid").dataSource.read();
            });
        };
    }
    angular.module("app")
        .directive("globalOptionRoleList", setupDirective);
}
