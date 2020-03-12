module Kitos.Organization.Users {
    "use strict";

    interface ICreateViewModel {
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
        hasApi: boolean;
    }

    class CreateOrganizationUserController {

        public busy: boolean;
        public vm: ICreateViewModel;
        public isUserGlobalAdmin = false;
        public isUserLocalAdmin = false;
        public isUserOrgAdmin = false;
        public isUserProjectAdmin = false;
        public isUserSystemAdmin = false;
        public isUserContractAdmin = false;
        public isUserReportAdmin = false;
        public hasApi = false;

        public static $inject: string[] = ["$uibModalInstance", "$http", "$q", "notify", "autofocus", "user", "_"];

        constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
            private $http: IHttpServiceWithCustomConfig,
            private $q: ng.IQService,
            private notify,
            private autofocus,
            private user: Kitos.Services.IUser,
            private _: _.LoDashStatic) {
            if (!user.currentOrganizationId) {
                notify.addErrorMessage("Fejl! Kunne ikke oprette bruger.", true);
                return;
            }

            this.isUserGlobalAdmin = user.isGlobalAdmin;
            this.isUserLocalAdmin = user.isLocalAdmin;
            this.isUserOrgAdmin = user.isOrgAdmin;
            this.isUserProjectAdmin = user.isProjectAdmin;
            this.isUserSystemAdmin = user.isSystemAdmin;
            this.isUserContractAdmin = user.isContractAdmin;
            this.isUserReportAdmin = user.isReportAdmin;
            this.hasApi = user.hasApi;

            autofocus();
            this.busy = false;
        }

        public cancel() {
            this.$uibModalInstance.close();
        }

        public create(sendMail: boolean) {
            this.busy = true;
            var userPayload: Models.ICreateUserPayload = {
                user: {
                    Name: this.vm.name,
                    LastName: this.vm.lastName,
                    Email: this.vm.email,
                    PhoneNumber: this.vm.phoneNumber,
                    HasApiAccess: this.vm.hasApi
        },
                organizationId: this.user.currentOrganizationId,
                sendMailOnCreation: sendMail
            };

            var msg = this.notify.addInfoMessage("Opretter bruger", false);

            this.$http.post<Models.IUser>("odata/Users/Users.Create", userPayload, { handleBusy: true })
                .then((response) => {
                    var userResult = response.data;

                    var promises: ng.IHttpPromise<any>[] = [];
                    promises.push(this.addRole(this.user.currentOrganizationId, userResult.Id, Models.OrganizationRole.User));
                    if (this.vm.isLocalAdmin)
                        promises.push(this.addRole(this.user.currentOrganizationId, userResult.Id, Models.OrganizationRole.LocalAdmin));
                    if (this.vm.isOrgAdmin)
                        promises.push(this.addRole(this.user.currentOrganizationId, userResult.Id, Models.OrganizationRole.OrganizationModuleAdmin));
                    if (this.vm.isProjectAdmin)
                        promises.push(this.addRole(this.user.currentOrganizationId, userResult.Id, Models.OrganizationRole.ProjectModuleAdmin));
                    if (this.vm.isSystemAdmin)
                        promises.push(this.addRole(this.user.currentOrganizationId, userResult.Id, Models.OrganizationRole.SystemModuleAdmin));
                    if (this.vm.isContractAdmin)
                        promises.push(this.addRole(this.user.currentOrganizationId, userResult.Id, Models.OrganizationRole.ContractModuleAdmin));
                    if (this.vm.isReportAdmin)
                        promises.push(this.addRole(this.user.currentOrganizationId, userResult.Id, Models.OrganizationRole.ReportModuleAdmin));

                    // when all requests are done
                    this.$q.all(promises).then(
                        () => {
                            msg.toSuccessMessage(`${this.vm.name} ${this.vm.lastName} er oprettet i KITOS`);
                            this.cancel();
                        },
                        () => msg.toErrorMessage(`Kunne ikke tilknytte ${this.vm.name} ${this.vm.lastName} til organisationen!`)); // on error: display error
                }, () => {
                    msg.toErrorMessage(`Fejl! Noget gik galt ved oprettelsen af ${this.vm.name} ${this.vm.lastName}!`);
                    this.cancel();
                }
            );
        }

        private addRole(organizationId: number, userId: number, role: Models.OrganizationRole): ng.IHttpPromise<Models.IOrganizationRight> {
            var rightsPayload: Models.IOrganizationRight = {
                UserId: userId,
                Role: role,
            };

            return this.$http.post<Models.IOrganizationRight>(`odata/Organizations(${organizationId})/Rights`, rightsPayload);
        }

        public attachUser() {
            var msg = this.notify.addInfoMessage("Tilknytter bruger", false);
            // TODO: When cannot handle + character in the email. It should be encoded as %2B. in order for the filter to function
            this.$http.get<Models.IODataResult<Models.IUser>>(`odata/Users?$filter=Email eq '${this.vm.email}'`).then((response) => {
                var userResult = this._.first(response.data.value);

                var promises: ng.IHttpPromise<any>[] = [];
                promises.push(this.addRole(this.user.currentOrganizationId, userResult.Id, Models.OrganizationRole.User));
                if (this.vm.isLocalAdmin)
                    promises.push(this.addRole(this.user.currentOrganizationId, userResult.Id, Models.OrganizationRole.LocalAdmin));
                if (this.vm.isOrgAdmin)
                    promises.push(this.addRole(this.user.currentOrganizationId, userResult.Id, Models.OrganizationRole.OrganizationModuleAdmin));
                if (this.vm.isProjectAdmin)
                    promises.push(this.addRole(this.user.currentOrganizationId, userResult.Id, Models.OrganizationRole.ProjectModuleAdmin));
                if (this.vm.isSystemAdmin)
                    promises.push(this.addRole(this.user.currentOrganizationId, userResult.Id, Models.OrganizationRole.SystemModuleAdmin));
                if (this.vm.isContractAdmin)
                    promises.push(this.addRole(this.user.currentOrganizationId, userResult.Id, Models.OrganizationRole.ContractModuleAdmin));
                if (this.vm.isReportAdmin)
                    promises.push(this.addRole(this.user.currentOrganizationId, userResult.Id, Models.OrganizationRole.ReportModuleAdmin));

                // when all requests are done
                this.$q.all(promises).then(
                    () => {
                        msg.toSuccessMessage(`${userResult.Name} ${userResult.LastName} er tilknyttet til organisationen`);
                        this.cancel();
                    },
                    (reason) => msg.toErrorMessage(`Kunne ikke tilknytte ${userResult.Name} ${userResult.LastName} til organisationen!`)); // on error: display error
            });
        }
    }

    angular
        .module("app")
        .config(["$stateProvider", ($stateProvider: ng.ui.IStateProvider) => {
            $stateProvider.state("organization.user.create", {
                url: "/create",
                onEnter: [
                    "$state", "$stateParams", "$uibModal",
                    ($state: ng.ui.IStateService, $stateParams: ng.ui.IStateParamsService, $uibModal: ng.ui.bootstrap.IModalService) => {
                        $uibModal.open({
                            templateUrl: "app/components/org/user/org-user-create.modal.view.html",
                            // fade in instead of slide from top, fixes strange cursor placement in IE
                            // http://stackoverflow.com/questions/25764824/strange-cursor-placement-in-modal-when-using-autofocus-in-internet-explorer
                            windowClass: "modal fade in",
                            controller: CreateOrganizationUserController,
                            controllerAs: "ctrl",
                            resolve: {
                                user: ["userService", userService => userService.getUser()]
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
