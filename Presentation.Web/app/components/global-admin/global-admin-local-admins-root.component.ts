module Kitos.GlobalAdmin.Components {

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                userId: "=",
            },
            controller: GlobalAdminLocalAdminRootController,
            controllerAs: "ctrl",
            templateUrl: `app/components/global-admin/global-admin-local-admins-root.view.html`
        };
    }
    
    interface IGlobalAdminLocalAdminRootController extends ng.IComponentController {
        userId: number
    }

    class GlobalAdminLocalAdminRootController implements IGlobalAdminLocalAdminRootController {
        userId: number | null = null;

        localAdmins: Array<Models.GlobalAdmin.ILocalAdminRow> = [];
        callbacks: IGlobalAdminLocalAdminCallbacks;

        newUser: Models.ViewModel.Generic.ISelect2Model<number>;
        newOrg: Models.ViewModel.Select2.ISelect2OrganizationOptionWithCvrViewModel;

        static $inject: string[] = ["userService", "organizationRightService", "select2LoadingService", "$scope"];
        constructor(
            private readonly userService: Services.IUserService,
            private readonly organizationRightService: Services.Organization.IOrganizationRightService,
            private readonly select2LoadingService: Services.ISelect2LoadingService,
            private readonly $scope: ng.IScope) {
        }
        
        $onInit() {
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

            const roleId = API.Models.OrganizationRole.LocalAdmin;

            if (!(userToUpdateId && newOrgId && roleId)) return;
            
            this.organizationRightService.create(newOrgId, userToUpdateId, roleId)
                .then(() => {
                    this.reauthorizeUserAndLoadData(userToUpdateId);
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

            return this.organizationRightService.remove(organizationId, roleId, userId)
                .then(() => {
                    this.reauthorizeUserAndLoadData(userId);
                });
        }
        
        getOrganizationSelectOptions() {
            return this.select2LoadingService.loadSelect2WithDataHandler("api/organization", true, ["take=100"], (item, items) => {
                items.push({
                    id: item.id,
                    text: item.name ? item.name : 'Intet navn',
                    cvr: item.cvrNumber
                });
            }, "q", Helpers.Select2OptionsFormatHelper.formatOrganizationWithCvr);
        } 

        private reauthorizeUserAndLoadData(userId: number) {
            if (userId === this.userId) {
                this.userService.reAuthorize();
            }
            this.loadData();
        }

        private loadData(): ng.IPromise<void> {
            if (this.localAdmins.length > 0) {
                this.resetLocalAdminData();
            }
            
            return this.organizationRightService.getAllByRightsType(API.Models.OrganizationRole.LocalAdmin).then(response => {
                this.localAdmins.pushArray(response.map((row, index) => {
                    return {
                        id: index,
                        name: row.user.fullName,
                        organization: row.organizationName,
                        email: row.userEmail,
                        objectContext: row
                    } as Models.GlobalAdmin.ILocalAdminRow;
                }));

                this.publishLocalAdminRightsUpdatedEvent();
            });
        }

        private resetLocalAdminData() {
            const arrayLength = this.localAdmins.length;
            this.localAdmins.splice(0, arrayLength);
        }

        private publishLocalAdminRightsUpdatedEvent() {
            this.$scope.$broadcast(Constants.LocalAdminListEvents.localAdminRightsUpdated);
        }
    }

    angular.module("app")
        .component("globalAdminLocalAdminsRoot", setupComponent());
}