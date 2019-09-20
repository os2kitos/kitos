module Kitos.GlobalAdmin.HelpTexts {
    "use strict";

    export interface IOverviewController {
        mainGrid: Kitos.IKendoGrid<Models.IHelpText>;
        mainGridOptions: kendo.ui.GridOptions;
    }

    export class HelpTextsController implements IOverviewController {
        public mainGrid: Kitos.IKendoGrid<Models.IHelpText>;
        public mainGridOptions: kendo.ui.GridOptions;

        public static $inject: Array<string> = [
            "$rootScope",
            "$scope",
            "$http",
            "$timeout",
            "$window",
            "$state",
            "$",
            "_",
            "notify",
            "helpTexts",
            "user",
            "$uibModal"
        ];

        constructor(
            private $rootScope: IRootScope,
            private $scope: ng.IScope,
            private $http: ng.IHttpService,
            private $timeout: ng.ITimeoutService,
            private $window: ng.IWindowService,
            private $state: ng.ui.IStateService,
            private $: JQueryStatic,
            private _: ILoDashWithMixins,
            private notify,
            private helpTexts,
            private user,
            private $modal) {
            this.$rootScope.page.title = "Hjælpetekster - Overblik";

            this.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: (options) => {
                                return "/odata/HelpTexts"
                            },
                            dataType: "json"
                        }
                    },
                    sort: {
                        field: "Title",
                        dir: "asc"
                    },
                    pageSize: 100,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true,
                },
                pageable: {
                    refresh: true,
                    pageSizes: [10, 25, 50, 100, 200],
                    buttonCount: 5
                },
                sortable: {
                    mode: "single"
                },
                reorderable: true,
                resizable: true,
                filterable: {
                    mode: "row",
                    messages: {
                        isTrue: "✔",
                        isFalse: "✖"
                    }
                },
                groupable: false,
                columnMenu: {
                    filterable: false
                },
                toolbar: [
                    {
                        //TODO ng-show='hasWriteAccess'
                        name: "opretHjælpeTekst",
                        text: "Opret hjælpetekst",
                        template: "<a ui-sref='global-admin.help-texts.create' class='btn btn-success pull-right'>#: text #</a>"
                    }
                ],
                columns: [
                    {
                        field: "Title", title: "Titel", width: 150,
                        persistId: "title", // DON'T YOU DARE RENAME!
                        template: dataItem => `<a ui-sref="global-admin.help-texts-edit({id:${dataItem.Id}})">${dataItem.Title}</a>`,
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
                        field: "Key", title: "Key", width: 150,
                        persistId: "key", // DON'T YOU DARE RENAME!
                        filterable: {
                            cell: {
                                template: customFilter,
                                dataSource: [],
                                showOperators: false,
                                operator: "contains"
                            }
                        }
                    }
                ]
            };
            function customFilter(args) {
                args.element.kendoAutoComplete({
                    noDataTemplate: ''
                });
            }
        }
        private reload() {
            this.$state.go(".", null, { reload: true });
        }
    }

    angular
        .module("app")
        .config([
            '$stateProvider', ($stateProvider) => {
                $stateProvider.state('global-admin.help-texts', {
                    url: '/help-texts',
                    templateUrl: 'app/components/global-admin/global-admin-help-texts.view.html',
                    controller: HelpTextsController,
                    controllerAs: 'helpTextsCtrl',
                    authRoles: ['GlobalAdmin'],
                    resolve: {
                        user: [
                            'userService', (userService) => {
                                return userService.getUser();
                            }
                        ],
                        helpTexts: [
                            "$http", $http => $http.get("/odata/HelpTexts").then(result => result.data.value)
                        ]
                    }
                });
            }
        ]);
}