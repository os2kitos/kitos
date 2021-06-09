module Kitos.Organization.Users {
    "use strict";

    interface IEditViewModel {
        name: string;
        email: string;
        lastName: string;
        phoneNumber: string;
        hasApi: boolean;
        isLocalAdmin: boolean;
        isOrgAdmin: boolean;
        isProjectAdmin: boolean;
        isSystemAdmin: boolean;
        isContractAdmin: boolean;
        isReportAdmin: boolean;
        isRightsHolder: boolean;
    }

    class EditOrganizationUserController {
        public vm: IEditViewModel;
        public isUserGlobalAdmin = false;
        public isUserLocalAdmin = false;
        public isUserOrgAdmin = false;
        public isUserProjectAdmin = false;
        public isUserSystemAdmin = false;
        public isUserContractAdmin = false;
        public isUserReportAdmin = false;
        public hasApi = false;

        private userId: number;
        private originalVm;

        public static $inject: string[] = ["$uibModalInstance", "$http", "$q", "notify", "user", "currentUser", "_"];

        constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private readonly $http: IHttpServiceWithCustomConfig,
            private readonly $q: ng.IQService,
            private readonly notify,
            private readonly user: Models.IUser,
            private readonly currentUser: Services.IUser,
            private readonly _: ILoDashWithMixins) {
            this.userId = user.Id;

            const hasRole = (role: Models.OrganizationRole) => {
                return _.find(user.OrganizationRights, { Role: role }) !== undefined;
            }

            const userVm: IEditViewModel = {
                email: user.Email,
                name: user.Name,
                hasApi: user.HasApiAccess,
                lastName: user.LastName,
                phoneNumber: user.PhoneNumber,
                isLocalAdmin: hasRole(Models.OrganizationRole.LocalAdmin),
                isOrgAdmin: hasRole(Models.OrganizationRole.OrganizationModuleAdmin),
                isProjectAdmin: hasRole(Models.OrganizationRole.ProjectModuleAdmin),
                isSystemAdmin: hasRole(Models.OrganizationRole.SystemModuleAdmin),
                isContractAdmin: hasRole(Models.OrganizationRole.ContractModuleAdmin),
                isReportAdmin: hasRole(Models.OrganizationRole.ReportModuleAdmin),
                isRightsHolder: hasRole(Models.OrganizationRole.RightsHolderAccess)
            };
            this.originalVm = _.clone(userVm);

            this.vm = userVm;
            this.hasApi = currentUser.hasApi;
            this.isUserGlobalAdmin = currentUser.isGlobalAdmin;
            this.isUserLocalAdmin = currentUser.isLocalAdmin;
            this.isUserOrgAdmin = currentUser.isOrgAdmin;
            this.isUserProjectAdmin = currentUser.isProjectAdmin;
            this.isUserSystemAdmin = currentUser.isSystemAdmin;
            this.isUserContractAdmin = currentUser.isContractAdmin;
            this.isUserReportAdmin = currentUser.isReportAdmin;
        }

        private changeRight(diffRights, property: string, role: Models.OrganizationRole): ng.IHttpPromise<any> {
            // check if the requested property exsists in the diff
            if (Object.keys(diffRights).indexOf(property) !== -1) {
                if (diffRights[property]) {
                    // add role to user
                    let payload: Models.IOrganizationRight = {
                        UserId: this.userId,
                        Role: role,
                        OrganizationId: this.currentUser.currentOrganizationId
                    };
                    return this.$http.post(`/odata/Organizations(${this.currentUser.currentOrganizationId})/Rights`, payload);
                } else {
                    // remove role from user
                    let rightsObj = this._.find(this.user.OrganizationRights, { Role: role });
                    return this.$http.delete(`/odata/Organizations(${this.currentUser.currentOrganizationId})/Rights(${rightsObj.Id})`);
                }
            }
        }

        public ok() {
            // get the changed values
            var diffRights: any = this._.omitBy(this.vm, (v, k) => this.originalVm[k] === v);

            var promises: ng.IHttpPromise<any>[] = [];
            promises.push(this.changeRight(diffRights, "isLocalAdmin", Models.OrganizationRole.LocalAdmin));
            promises.push(this.changeRight(diffRights, "isOrgAdmin", Models.OrganizationRole.OrganizationModuleAdmin));
            promises.push(this.changeRight(diffRights, "isProjectAdmin", Models.OrganizationRole.ProjectModuleAdmin));
            promises.push(this.changeRight(diffRights, "isSystemAdmin", Models.OrganizationRole.SystemModuleAdmin));
            promises.push(this.changeRight(diffRights, "isContractAdmin", Models.OrganizationRole.ContractModuleAdmin));
            promises.push(this.changeRight(diffRights, "isReportAdmin", Models.OrganizationRole.ReportModuleAdmin));
            promises.push(this.changeRight(diffRights, "isRightsHolder", Models.OrganizationRole.RightsHolderAccess));

            var payload: Models.IUser = {
                Name: this.vm.name,
                LastName: this.vm.lastName,
                PhoneNumber: this.vm.phoneNumber,
                Email: this.vm.email,
                HasApiAccess: this.vm.hasApi


            };
            this.$http.patch(`/odata/Users(${this.userId})`, payload);

            // when all requests are done
            this.$q.all(promises).then(
                () => this.$uibModalInstance.close(), // on success: close the modal
                () => this.notify.addErrorMessage("Fejl. Noget gik galt!")); // on error: display error
        }

        public cancel() {
            this.$uibModalInstance.dismiss();
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("organization.user.edit", {
                url: "/:id/edit",
                onEnter: [
                    "$state", "$stateParams", "$uibModal",
                    ($state: ng.ui.IStateService, $stateParams: ng.ui.IStateParamsService, $uibModal: ng.ui.bootstrap.IModalService) => {
                        $uibModal.open({
                            templateUrl: "app/components/org/user/org-user-edit.modal.view.html",
                            // fade in instead of slide from top, fixes strange cursor placement in IE
                            // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                            windowClass: "modal fade in",
                            controller: EditOrganizationUserController,
                            controllerAs: "ctrl",
                            resolve: {
                                currentUser: ["userService",
                                    (userService) => userService.getUser()
                                ],
                                user: ["$http", "userService",
                                    ($http: ng.IHttpService, userService) =>
                                        userService.getUser().then((currentUser) =>
                                            $http.get(`/odata/Users(${$stateParams["id"]})?$expand=OrganizationRights($filter=OrganizationId eq ${currentUser.currentOrganizationId})`)
                                                .then((response) => response.data))
                                ]
                            }
                        }).result.then(() => {
                            // OK
                            // GOTO parent state and reload
                            $state.go("^", null, { reload: true });
                        },
                            () => {
                                // Cancel
                                // GOTO parent state
                                $state.go("^");
                            });
                    }
                ]
            });
        }]);
}
