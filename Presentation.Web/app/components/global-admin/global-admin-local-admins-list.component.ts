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
        organization: string,
        name: string,
        email: string,
        objectContext: Models.Api.Organization.ILocalAdminRightsDto,
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
            var users = this.localAdmins;
            this.$scope.localAdminListVm = {
                removeRight: (dataItem: ILocalAdminRow) => this.deleteLocalAdmin(dataItem.objectContext),
            };

            this.userService.getUser().then(user => {
                this.kendoGridLauncherFactory
                    .create<ILocalAdminRow>()
                    .withUser(user)
                    .withGridBinding(this)
                    .withFlexibleWidth()
                    .withArrayDataSource(this.localAdmins.map(row => {
                        return <ILocalAdminRow>{
                            name: row.user.fullName,
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
                        .withTitle("Slet")
                        .withRendering((source: ILocalAdminRow) => ` <button type='button' data-element-type='deleteLocalAdminRight' data-confirm-click="Er du sikker på at du vil slette?" class='btn btn-link' title='Slet reference' data-confirmed-click='localAdminListVm.removeRight(${source})'><i class='fa fa-trash-o'  aria-hidden='true'></i></button>`)
                    )
                    .resetAnySavedSettings()
                    .launch();
            });
        }

        private deleteLocalAdmin = (right: Models.Api.Organization.ILocalAdminRightsDto) => {
            var oId = right.organizationId;
            var rId = right.role;
            var uId = right.userId;
            //var msg = notify.addInfoMessage("Arbejder ...", false);
            /*$http.delete("api/OrganizationRight/" + oId + "?rId=" + rId + "&uId=" + uId + "&organizationId=" + user.currentOrganizationId)
                .then(function onSuccess(result) {
                    msg.toSuccessMessage(right.userName + " er ikke længere lokal administrator");
                    if (uId == user.id) {
                        // Reload user
                        userService.reAuthorize();
                    }
                    reload();
                }, function onError(result) {

                    msg.toErrorMessage("Kunne ikke fjerne " + right.userName + " som lokal administrator");
                });*/
        }
    }

    angular.module("app")
        .component("globalAdminLocalAdminsList", setupComponent());
}