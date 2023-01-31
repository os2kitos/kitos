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

        newUser: Models.ViewModel.Select2.IBaseSelect2OptionViewModel;
        newOrg: Models.ViewModel.Select2.ISelect2OrganizationOptionWithCvrViewModel;

        static $inject: string[] = ["userService", "organizationRightService", "select2LoadingService", "$scope", "notify"];
        constructor(
            private readonly userService: Services.IUserService,
            private readonly organizationRightService: Services.Organization.IOrganizationRightService,
            private readonly select2LoadingService: Services.ISelect2LoadingService,
            private readonly $scope: ng.IScope,
            private readonly notify) {
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

            const userToUpdateId = this.newUser.id;
            const newOrgId = this.newOrg.id;
            var userName = this.newUser.text;
            var orgName = this.newOrg.text;

            const roleId = API.Models.OrganizationRole.LocalAdmin;

            if (!(userToUpdateId && newOrgId && roleId)) return;
            
            var msg = this.notify.addInfoMessage("Arbejder ...", false);
            const self = this;
            this.organizationRightService.create(newOrgId, userToUpdateId, roleId)
                .then(() => {
                    msg.toSuccessMessage(userName + " er blevet lokal administrator for " + orgName);
                    if (self.userId === userToUpdateId) {
                        // Reload user
                        self.userService.reAuthorize();
                    }

                    self.loadData();
                }, () => {
                    msg.toErrorMessage("Kunne ikke gøre " + userName + " til lokal administrator for " + orgName);
                });

            this.newOrg = null;
            this.newUser = null;
        }

        deleteLocalAdmin(rightId: number) {
            const localAdminRow = _.find(this.localAdmins, { id: rightId });
            if (!localAdminRow)
                return null;

            const right = localAdminRow.objectContext;

            const organizationId = right.organizationId;
            const roleId = right.role;
            var userId = right.userId;

            return this.organizationRightService.remove(this.currentOrganizationId, organizationId, roleId, userId)
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
            if (!this.localAdmins) {
                this.localAdmins = [];
            } else {
                this.resetLocalAdminData();
            }

            return this.organizationRightService.getAll().then(response => {
                this.localAdmins.pushArray(response.map((row, index) => {
                    return {
                        id: index,
                        name: row.user.fullName,
                        organization: row.organizationName,
                        email: row.userEmail,
                        objectContext: row
                    } as ILocalAdminRow;
                }));

                this.emitLocalAdminRightsUpdatedEvent();
            });
        }

        private resetLocalAdminData() {
            const arrayLength = this.localAdmins.length;
            this.localAdmins.splice(0, arrayLength);
        }

        private emitLocalAdminRightsUpdatedEvent() {
            this.$scope.$broadcast("LocalAdminRights_Updated");
        }
    }

    angular.module("app")
        .component("globalAdminLocalAdminsRoot", setupComponent());
}