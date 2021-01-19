module Kitos.LocalAdmin.Directives {
    "use strict";

    function setupDirective(): ng.IDirective {
        return {
            scope: {
                editState: "@state",
                dirId: "@",
                optionType: "@",
                currentOrgId: "@"
            },
            controller: LocalOptionListDirective,
            controllerAs: "ctrl",
            bindToController: {
                optionsUrl: "@",
                title: "@"
            },
            template: `<h2>{{ ctrl.title }}</h2><div id="{{ ctrl.dirId }}" data-kendo-grid="{{ ctrl.mainGrid }}" data-k-options="{{ ctrl.mainGridOptions }}"></div>`
        };
    }

    interface IDirectiveScope {
        title: string;
        editState: string;
        optionsUrl: string;
        optionId: string;
        optionType: string;
        dirId: string;
    }

    class LocalOptionListDirective implements IDirectiveScope {
        public optionsUrl: string;
        public title: string;
        public editState: string;
        public optionId: string;
        public dirId: string;
        public optionType: string;

        public mainGrid: IKendoGrid<Models.IOptionEntity>;
        public mainGridOptions: IKendoGridOptions<Models.IOptionEntity>;

        public static $inject: string[] = ["$", "$state", "$scope", "localOptionUrlResolver"];

        constructor(
            private $: JQueryStatic,
            private $state: ng.ui.IStateService,
            private $scope,
            private localOptionUrlResolver: Kitos.Services.LocalOptions.ILocalOptionUrlResolver) {

            this.$scope.$state = $state;
            this.editState = $scope.editState;
            this.dirId = $scope.dirId;
            this.optionType = $scope.optionType;

            this.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: localOptionUrlResolver.resolveKendoGridGetUrl(parseInt(this.optionType.toString()), $scope.currentOrgId),
                            dataType: "json"
                        }
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
                editable: true,
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
                        field: "IsLocallyAvailable", title: "Aktiv", width: 112,
                        persistId: "isLocallyAvailable", // DON'T YOU DARE RENAME!
                        attributes: { "class": "text-center" },
                        template: `# if(IsObligatory) { # <span class="glyphicon glyphicon-check text-grey" aria-hidden="true"></span> # } else { # <input type="checkbox" data-ng-model="dataItem.IsLocallyAvailable" data-global-option-id="{{ dataItem.Id }}" data-autosave="${localOptionUrlResolver.resolveAutosaveUrl(parseInt(this.optionType.toString()))}" data-field="OptionId"> # } #`,
                        hidden: false,
                        filterable: false,
                        sortable: false
                    },
                    {
                        field: "Name", title: "Navn", width: 230,
                        persistId: "name", // DON'T YOU DARE RENAME!
                        template: (dataItem) => dataItem.Name,
                        hidden: false,
                        filterable: {
                            cell: {
                                template: customFilter,
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
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    },
                    {
                        name: "editOption",
                        text: "Redigér",
                        template: "<button type='button' class='btn btn-link' title='Redigér type' ng-click='ctrl.editOption($event)'><i class='fa fa-pencil' aria-hidden='true'></i></button>",
                        title: " ",
                        width: 176
                    } as any
                ]
            };
            function customFilter(args) {
                args.element.kendoAutoComplete({
                    noDataTemplate: ''
                });
            }
        }
        //NOTE: Referenced from kendo configuration above - static analysis will say it is unused
        private editOption = (e: JQueryEventObject) => {
            e.preventDefault();
            var entityGrid = this.$(`#${this.dirId}`).data("kendoGrid");
            var selectedItem = entityGrid.dataItem(this.$(e.currentTarget).closest("tr"));
            this.optionId = selectedItem.get("id");
            this.$scope.$state.go(this.editState, { id: this.optionId, optionType: this.optionType });
        }
    }
    angular.module("app")
        .directive("localOptionList", setupDirective);
}