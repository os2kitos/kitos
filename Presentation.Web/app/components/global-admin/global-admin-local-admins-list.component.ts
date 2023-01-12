module Kitos.GlobalAdmin.Components {

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                localAdmins: "<"
            },
            controller: GlobalAdminLocalAdminListController,
            controllerAs: "ctrl",
            templateUrl: `app/components/global-admin/global-admin-local-admins-list.view.html`
        };
    }

    export interface ILocalAdminRow {
        organization: string
        name: string
        email: string,
        objectContext: Models.Api.Organization.ILocalAdminRightsDto
    }

    interface IGlobalAdminLocalAdminListController extends ng.IComponentController, Utility.KendoGrid.IGridViewAccess<ILocalAdminRow> {
        localAdmins: Array<Models.Api.Organization.ILocalAdminRightsDto>;
    }

    class GlobalAdminLocalAdminListController implements IGlobalAdminLocalAdminListController
    {
        localAdmins: Array<Models.Api.Organization.ILocalAdminRightsDto> | null = null;
        mainGrid: IKendoGrid<ILocalAdminRow>;
        mainGridOptions: IKendoGridOptions<ILocalAdminRow>;

        static $inject: string[] = ["kendoGridLauncherFactory", "$scope", "userService"];
        constructor(
            private readonly kendoGridLauncherFactory: Utility.KendoGrid.IKendoGridLauncherFactory,
            private readonly $scope: ng.IScope,
            private readonly userService: Kitos.Services.IUserService) {
        }


        $onInit() {
            if (this.localAdmins) {
                this.loadGrid();
            } else {
                console.error("Missing parameter 'localAdmins'");
            }
        }

        private loadGrid() {

            this.userService.getUser().then(user => {
                this.kendoGridLauncherFactory
                    .create<ILocalAdminRow>()
                    .withUser(user)
                    .withGridBinding(this)
                    .withFlexibleWidth()
                    .withArrayDataSource(this.localAdmins.map(row => {
                        return <ILocalAdminRow>{
                            name: row.fullName,
                            organization: row.organizationName,
                            email: row.userEmail,
                            objectContext: row
                        };
                    }))
                    .withEntityTypeName("FK Organisation - Konsekvenser ved opdatering")
                    .withStorageKey("fkOrgConsequences")
                    .withScope(this.$scope)
                    .withoutPersonalFilterOptions()
                    .withoutGridResetControls()
                    .withDefaultPageSize(10)
                    .withColumn(builder => builder
                        .withId("organization")
                        .withDataSourceName("organization")
                        .withTitle("Organisationsenhed")
                        .withSourceValueEchoRendering()
                        .withFilteringOperation(Kitos.Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                    )
                    .withColumn(builder => builder
                        .withId("name")
                        .withDataSourceName("name")
                        .withTitle("Navn")
                        .withSourceValueEchoRendering()
                        .withFilteringOperation(Kitos.Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                    )
                    .withColumn(builder => builder
                        .withId("email")
                        .withDataSourceName("email")
                        .withTitle("Email")
                        .withSourceValueEchoRendering()
                        .withFilteringOperation(Kitos.Utility.KendoGrid.KendoGridColumnFiltering.Contains)
                    )
                    .resetAnySavedSettings()
                    .launch();
            });
        }
    }

    angular.module("app")
        .component("globalAdminLocalAdminList", setupComponent());
}