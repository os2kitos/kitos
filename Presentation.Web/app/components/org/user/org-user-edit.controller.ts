module Kitos.Organization.Users {
    "use strict";

    interface IEditViewModel {
        name: string;
        email: string;
        lastName: string;
        phoneNumber: string;
        isLocalAdmin: boolean;
        isOrgAdmin: boolean;
        isProjectAdmin: boolean;
        isSystemAdmin: boolean;
        isContractAdmin: boolean;
        isReportAdmin: boolean;
        isReadOnly: boolean;
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
        public isUserReadOnly = false;

        private userId: number;
        private originalVm;

        public static $inject: string[] = ["$uibModalInstance", "$http", "$q", "notify", "user", "currentUser", "_"];

        constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private $http: IHttpServiceWithCustomConfig,
            private $q: ng.IQService,
            private notify,
            private user: Models.IUser,
            private currentUser: Services.IUser,
            private _: ILoDashWithMixins) {
            this.userId = user.Id;
            var userVm: IEditViewModel = {
                email: user.Email,
                name: user.Name,
                lastName: user.LastName,
                phoneNumber: user.PhoneNumber,
                isLocalAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.LocalAdmin }) !== undefined,
                isOrgAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.OrganizationModuleAdmin }) !== undefined,
                isProjectAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.ProjectModuleAdmin }) !== undefined,
                isSystemAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.SystemModuleAdmin }) !== undefined,
                isContractAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.ContractModuleAdmin }) !== undefined,
                isReportAdmin: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.ReportModuleAdmin }) !== undefined,
                isReadOnly: _.find(user.OrganizationRights, { Role: Models.OrganizationRole.ReadOnly }) !== undefined,
                hasApi: _.find(user.OrganizationRights, {Role: Models.OrganizationRole.ApiAccess }) !== undefined
            };
            this.originalVm = _.clone(userVm);
            this.vm = userVm;

            this.isUserGlobalAdmin = currentUser.isGlobalAdmin;
            this.isUserLocalAdmin = currentUser.isLocalAdmin;
            this.isUserOrgAdmin = currentUser.isOrgAdmin;
            this.isUserProjectAdmin = currentUser.isProjectAdmin;
            this.isUserSystemAdmin = currentUser.isSystemAdmin;
            this.isUserContractAdmin = currentUser.isContractAdmin;
            this.isUserReportAdmin = currentUser.isReportAdmin;
            this.isUserReadOnly = currentUser.isReadOnly;
        }

        private changeRight(diffRights, property: string, role: Models.OrganizationRole): ng.IHttpPromise<any> {
            // check if the requested property exsists in the diff
            if (Object.keys(diffRights).indexOf(property) === -1)
                return; // if it doesn't then it wasn't changed and we abort

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
            promises.push(this.changeRight(diffRights, "isReadOnly", Models.OrganizationRole.ReadOnly));
            promises.push(this.changeRight(diffRights, "hasApi", Models.OrganizationRole.ApiAccess));

            var payload: Models.IUser = {
                Name: this.vm.name,
                LastName: this.vm.lastName,
                PhoneNumber: this.vm.phoneNumber,
                Email: this.vm.email
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
