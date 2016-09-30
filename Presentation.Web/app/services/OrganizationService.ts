module Kitos.Services {
    "use strict";

    interface IOrganizationServiceVm {
        email: string;
        isLocalAdmin: boolean;
        isOrgAdmin: boolean;
        isProjectAdmin: boolean;
        isSystemAdmin: boolean;
        isContractAdmin: boolean;
        isReportAdmin: boolean;
    }

    export class OrganizationService {

        public vm: IOrganizationServiceVm;
        public isUserGlobalAdmin = false;
        public isUserLocalAdmin = false;
        public isUserOrgAdmin = false;
        public isUserProjectAdmin = false;
        public isUserSystemAdmin = false;
        public isUserContractAdmin = false;
        public isUserReportAdmin = false;

        public static $inject: string[] = ["$http", "$q", "notify", "user", "_"];

        constructor(private $http: IHttpServiceWithCustomConfig,
            private $q: ng.IQService,
            private notify,
            private user: Services.IUser,
            private _: _.LoDashStatic) {

            this.isUserGlobalAdmin = user.isGlobalAdmin;
            this.isUserLocalAdmin = user.isLocalAdmin;
            this.isUserOrgAdmin = user.isOrgAdmin;
            this.isUserProjectAdmin = user.isProjectAdmin;
            this.isUserSystemAdmin = user.isSystemAdmin;
            this.isUserContractAdmin = user.isContractAdmin;
            this.isUserReportAdmin = user.isReportAdmin;
        }

        private addRole(organizationId: number, userId: number, role: Models.OrganizationRole): ng.IHttpPromise<Models.IOrganizationRight> {
            var rightsPayload: Models.IOrganizationRight = {
                UserId: userId,
                Role: role
            };

            return this.$http.post<Models.IOrganizationRight>(`odata/Organizations(${organizationId})/Rights`, rightsPayload);
        }

        public attachUser(organizationId: number, userId: number, role: Models.OrganizationRole) {
            var msg = this.notify.addInfoMessage("Tilknytter bruger", false);

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
                        //this.cancel();
                    },
                    (reason) => msg.toErrorMessage(`Kunne ikke tilknytte ${userResult.Name} ${userResult.LastName} til organisationen!`)); // on error: display error
            });
        }
    }

    app.service("organizationService", OrganizationService);
}
