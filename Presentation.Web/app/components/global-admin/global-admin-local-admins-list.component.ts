module Kitos.GlobalAdmin.Components {

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                localAdmins: "<",
                callbacks: "<"
            },
            controller: GlobalAdminLocalAdminListController,
            controllerAs: "vm",
            templateUrl: `app/components/global-admin/global-admin-local-admins-list.view.html`
        };
    }

    export interface IGlobalAdminLocalAdminCallbacks {
        removeRight(rightId: number): ng.IPromise<void>;
    }

    interface IGlobalAdminLocalAdminListController extends ng.IComponentController, Utility.KendoGrid.IGridViewAccess<Models.GlobalAdmin.ILocalAdminRow> {
        localAdmins: Array<Models.GlobalAdmin.ILocalAdminRow>;
        callbacks: IGlobalAdminLocalAdminCallbacks;
    }

    class GlobalAdminLocalAdminListController implements IGlobalAdminLocalAdminListController
    {
        localAdmins: Array<Models.GlobalAdmin.ILocalAdminRow> | null = null;
        callbacks: IGlobalAdminLocalAdminCallbacks | null = null;

        mainGrid: IKendoGrid<Models.GlobalAdmin.ILocalAdminRow>;
        mainGridOptions: IKendoGridOptions<Models.GlobalAdmin.ILocalAdminRow>;

        static $inject: string[] = ["kendoGridLauncherFactory", "$scope", "userService"];
        constructor(
            private readonly kendoGridLauncherFactory: Utility.KendoGrid.IKendoGridLauncherFactory,
            private readonly $scope: ng.IScope,
            private readonly userService: Services.IUserService) {
        }


        $onInit() {
            if (this.localAdmins === null) {
                throw "Missing parameter 'localAdmins'";
            }
            if (this.callbacks === null) {
                throw "Missing parameter 'callbacks'";
            }

            this.loadGrid();
        }

        private loadGrid() {

            this.userService.getUser().then(user => {
                this.kendoGridLauncherFactory
                    .create<Models.GlobalAdmin.ILocalAdminRow>()
                    .withUser(user)
                    .withGridBinding(this)
                    .withFlexibleWidth()
                    .withArrayDataSource(this.localAdmins)
                    .withEntityTypeName("Lokale administratorer")
                    .withStorageKey("globalAdminLocalAdmins")
                    .withScope(this.$scope)
                    .withoutPersonalFilterOptions()
                    .withoutGridResetControls()
                    .withDefaultPageSize(50)
                    .withColumn(builder => builder
                        .withId("organization")
                        .withDataSourceName("organization")
                        .withTitle("Organisation")
                        .withSourceValueEchoRendering()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                     )
                    .withColumn(builder => builder
                        .withId("name")
                        .withDataSourceName("name")
                        .withTitle("Navn")
                        .withSourceValueEchoRendering()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                    )
                    .withColumn(builder => builder
                        .withId("email")
                        .withDataSourceName("email")
                        .withTitle("Email")
                        .withSourceValueEchoRendering()
                        .withFilteringOperation(Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                    )
                    .withColumn(builder => builder
                        .withId("deleteButton")
                        .withDataSourceName("deleteButton")
                        .withTitle(" ")
                        .withStandardWidth(30)
                        .withoutSorting()
                        .withoutVisibilityToggling()
                        .asUiOnlyColumn()
                        .withRendering(
                            (source: Models.GlobalAdmin.ILocalAdminRow) =>
                            `<button type='button' data-element-type='deleteLocalAdminRight' 
                            data-confirm-click="Er du sikker på at du vil slette?" data-confirmed-click='vm.deleteRight(${source.id})' 
                            class='k-button k-button-icontext' title='Slet reference'>Slet</button>`)
                    )
                    .resetAnySavedSettings()
                    .launch();

                var self = this;
                this.$scope.$on(Constants.LocalAdminListEvents.localAdminRightsUpdated, (evt, data) => {
                    self.mainGrid?.dataSource?.read();
                });
            });

        }

        deleteRight = (rightId: number) => {
            this.callbacks.removeRight(rightId);
        }
    }

    angular.module("app")
        .component("globalAdminLocalAdminsList", setupComponent());
}