module Kitos.GlobalAdmin.Components {

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                localAdmins: "<",
                currentOrganizationId: "=",
                userId: "=",
            },
            controller: GlobalAdminLocalAdminListController,
            controllerAs: "ctrl",
            templateUrl: `app/components/global-admin/global-admin-local-admins-list.view.html`
        };
    }

    export interface ILocalAdminRow {
        id: number,
        organization: string,
        name: string,
        email: string,
        currentOrgId: number,
        currentUserId: number,
        objectContext: Models.Api.Organization.ILocalAdminRightsDto,
    }

    interface IGlobalAdminLocalAdminListController extends ng.IComponentController, Utility.KendoGrid.IGridViewAccess<ILocalAdminRow> {
        localAdmins: Array<Models.Api.Organization.ILocalAdminRightsDto>;
        currentOrganizationId: number;
        userId: number;
    }

    class GlobalAdminLocalAdminListController implements IGlobalAdminLocalAdminListController
    {
        localAdmins: Array<Models.Api.Organization.ILocalAdminRightsDto> | null = null;
        currentOrganizationId: number | null = null;
        userId: number | null = null;

        mainGrid: IKendoGrid<ILocalAdminRow>;
        mainGridOptions: IKendoGridOptions<ILocalAdminRow>;

        private rowData: Array<ILocalAdminRow>;

        static $inject: string[] = ["kendoGridLauncherFactory", "$scope", "userService", "organizationRightService"];
        constructor(
            private readonly kendoGridLauncherFactory: Utility.KendoGrid.IKendoGridLauncherFactory,
            private readonly $scope,
            private readonly userService: Kitos.Services.IUserService,
            private readonly organizationRightService: Services.Organization.IOrganizationRightService) {
        }


        $onInit() {
            if (this.localAdmins === null) {
                throw "Missing parameter 'localAdmins'";
            }
            if(this.currentOrganizationId === null){
                throw "Missing parameter 'currentOrganizationId'";
            }
            if (this.userId === null){
                throw "Missing parameter 'userId'";
            }

            this.loadGrid();
        }

        private loadGrid() {

            this.rowData = this.localAdmins.map((row, index) => {
                return <ILocalAdminRow>{
                    id: index,
                    name: row.user.fullName,
                    organization: row.organizationName,
                    email: row.userEmail,
                    currentOrgId: this.currentOrganizationId,
                    currentUserId: this.userId,
                    objectContext: row
                };
            });
            
            this.$scope.deleteRightMethod = this.deleteLocalAdmin;

            this.userService.getUser().then(user => {
                this.kendoGridLauncherFactory
                    .create<ILocalAdminRow>()
                    .withUser(user)
                    .withGridBinding(this)
                    .withFlexibleWidth()
                    .withArrayDataSource(this.rowData)
                    .withEntityTypeName("FK Organisation - Konsekvenser ved opdatering")
                    .withStorageKey("fkOrgConsequences")
                    .withScope(this.$scope)
                    .withoutPersonalFilterOptions()
                    .withoutGridResetControls()
                    .withDefaultPageSize(50)
                    .withColumn(builder => builder
                        .withId("organization")
                        .withDataSourceName("organization")
                        .withTitle("Organisationsenhed")
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
                        .withoutSorting()
                        .withoutMenu()
                        .withUiOnlyColumn()
                        .withRendering(
                            (source: ILocalAdminRow) =>
                            `<button type='button' data-element-type='deleteLocalAdminRight' 
                            data-confirm-click="Er du sikker på at du vil slette?" data-confirmed-click='deleteRightMethod(${source.id})' 
                            class='k-button k-button-icontext' title='Slet reference'>Slet</button>`)
                    )
                    .resetAnySavedSettings()
                    .launch();
            });
        }

        private deleteLocalAdmin = (rightId: number) => {
            var rightRow = _.find(this.rowData, { id: rightId });
            if (!rightRow)
                return;

            var right = rightRow.objectContext;

            var organizationId = right.organizationId;
            var roleId = right.role;
            var userId = right.userId;

            this.organizationRightService.removeRight(rightRow.currentOrgId, organizationId, roleId, userId)
                .then(() => {
                    if (userId === rightRow.currentUserId) {
                        this.userService.reAuthorize();
                    }

                    this.rowData.splice(rightId, 1);
                    this.mainGrid.dataSource.read();
                });
        }
    }

    angular.module("app")
        .component("globalAdminLocalAdminsList", setupComponent());
}