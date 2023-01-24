module Kitos.GlobalAdmin.Components {

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                currentOrganizationId: "=",
                userId: "=",
            },
            controller: GlobalAdminLocalAdminRootController,
            controllerAs: "ctrl",
            templateUrl: `app/components/global-admin/global-admin-local-admins-root.view.html`
        };
    }

    export interface ILocalAdminRow {
        id: number,
        organization: string,
        name: string,
        email: string,
        objectContext: Models.Api.Organization.ILocalAdminRightsDto,
    }
    
    interface IGlobalAdminLocalAdminRootController extends ng.IComponentController {
        currentOrganizationId: number;
        userId: number;
    }

    class GlobalAdminLocalAdminRootController implements IGlobalAdminLocalAdminRootController {
        currentOrganizationId: number | null = null;
        userId: number | null = null;

        localAdmins: Array<ILocalAdminRow>;
        callbacks: IGlobalAdminLocalAdminCallbacks;

        newUser: any;
        newOrg: any;

        static $inject: string[] = ["userService", "organizationRightService", "select2LoadingService", "notify", "$http", "$state"];
        constructor(
            private readonly userService: Services.IUserService,
            private readonly organizationRightService: Services.Organization.IOrganizationRightService,
            private readonly select2LoadingService: Services.ISelect2LoadingService,
            private readonly notify,
            private readonly $http) {
        }
        
        $onInit() {
            if (this.currentOrganizationId === null) {
                throw "Missing parameter 'currentOrganizationId'";
            }
            if (this.userId === null) {
                throw "Missing parameter 'userId'";
            }

            this.callbacks = {
                removeRight: (rightId: number) => this.deleteLocalAdmin(rightId)
            } as IGlobalAdminLocalAdminCallbacks;

            this.loadData();
        }

        newLocalAdmin() {
            // select2 changes the value twice, first with invalid values
            // so ignore invalid values
            if (typeof this.newUser !== "object") return;
            if (!(this.newOrg && this.newUser)) return;

            var user = this.newUser;
            var uId = user.id;
            const oId = this.newOrg.id;
            var orgName = this.newOrg.text;

            const rId = API.Models.OrganizationRole.LocalAdmin;

            if (!(uId && oId && rId)) return;

            const data = {
                userId: uId,
                role: rId,
                organizationId: oId
            };
            var msg = this.notify.addInfoMessage("Arbejder ...", false);
            const self = this;
            this.$http.post(`api/OrganizationRight/${oId}`, data, { handleBusy: true })
                .then(function onSuccess(result) {
                    msg.toSuccessMessage(user.text + " er blevet lokal administrator for " + orgName);
                    if (uId === user.id) {
                        // Reload user
                        self.userService.reAuthorize();
                    }
                    self.loadData();
                }, function onError(result) {
                    msg.toErrorMessage("Kunne ikke gøre " + user.text + " til lokal administrator for " + orgName);
                });
        }

        deleteLocalAdmin(rightId: number) {
            const localAdminRow = _.find(this.localAdmins, { id: rightId });
            if (!localAdminRow)
                return null;

            const right = localAdminRow.objectContext;

            const organizationId = right.organizationId;
            const roleId = right.role;
            var userId = right.userId;

            return this.organizationRightService.removeRight(this.currentOrganizationId, organizationId, roleId, userId)
                .then(() => {
                    if (userId === this.userId) {
                        this.userService.reAuthorize();
                    }
                    this.loadData();
                });
        }
        
        getOrganizationSelectOptions() {
            return this.select2LoadingService.loadSelect2WithDataHandler("api/organization", true, ["take=100", "orgId=" + this.currentOrganizationId], (item, items) => {
                items.push({
                    id: item.id,
                    text: item.name ? item.name : 'Unavngiven',
                    cvr: item.cvrNumber
                });
            }, "q", Helpers.Select2OptionsFormatHelper.formatOrganizationWithCvr);
        } 

        private loadData(): ng.IPromise<void> {
            this.localAdmins = [];
            return this.organizationRightService.getAll().then(localAdmins => {
                this.localAdmins.pushArray(localAdmins.map((row, index) => {
                    return {
                        id: index,
                        name: row.user.fullName,
                        organization: row.organizationName,
                        email: row.userEmail,
                        objectContext: row
                    } as ILocalAdminRow;
                }));
            });
        }
    }

    angular.module("app")
        .component("globalAdminLocalAdminsRoot", setupComponent());
}