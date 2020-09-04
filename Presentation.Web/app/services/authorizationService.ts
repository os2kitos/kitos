module Kitos.Services.Authorization {
    import EntitiesAccessRightsDTO = Models.Api.Authorization.EntitiesAccessRightsDTO;
    import EntityAccessRightsDTO = Models.Api.Authorization.EntityAccessRightsDTO;

    export interface IOperationAuthorizationService {
        //Gets top-level authorization (CREATE/READ)
        getOverviewAuthorization(): ng.IPromise<EntitiesAccessRightsDTO>;

        //Gets item-level authorization (READ/UPDATE/DELETE)
        getAuthorizationForItem(id: number): ng.IPromise<EntityAccessRightsDTO>;

        //Gets item-level authorizations (READ/UPDATE/DELETE)
        getAuthorizationForItems(ids: number[]): ng.IPromise<EntityAccessRightsDTO[]>;
    }

    class OperationAuthorizationService implements IOperationAuthorizationService {
        constructor(
            private readonly $http: ng.IHttpService,
            private readonly userService: Services.IUserService,
        private readonly entityType: string) {
        }

        private getBasePath() {
            return `api/${this.entityType}`;
        }

        getOverviewAuthorization(): ng.IPromise<EntitiesAccessRightsDTO> {
            return this
                .userService
                .getUser()
                .then((user : Services.IUser)=>
                    this.$http.get(`${this.getBasePath()}/?organizationId=${user.currentOrganizationId}&getEntitiesAccessRights=true`)
                        .then(result => {
                            var authResponse = result.data as { msg: string, response: EntitiesAccessRightsDTO };
                            return authResponse.response;
                        }
                    )
            );
        }

        getAuthorizationForItem(id: number): ng.IPromise<EntityAccessRightsDTO> {
            return this.$http.get(`${this.getBasePath()}/?id=${id}&getEntityAccessRights=true`)
                .then(result => {
                    var authResponse = result.data as { msg: string, response: EntityAccessRightsDTO };
                    return authResponse.response;
                });
        }

        getAuthorizationForItems(ids: number[]): ng.IPromise<EntityAccessRightsDTO[]> {
            return this.$http.post(`${this.getBasePath()}/?getEntityListAccessRights=true`, ids)
                .then((rightsResponse) => {
                    var accessControlResults = rightsResponse.data as { msg: string, response: EntityAccessRightsDTO[] };
                    return accessControlResults.response;
                });
        }
    }

    export interface IAuthorizationServiceFactory {
        createOrganizationAuthorization(): IOperationAuthorizationService;
        createOrganizationUnitAuthorization(): IOperationAuthorizationService;
        createSystemAuthorization(): IOperationAuthorizationService;
        createSystemUsageAuthorization(): IOperationAuthorizationService;
        createReportAuthorization(): IOperationAuthorizationService;
        createContractAuthorization(): IOperationAuthorizationService;
        createProjectAuthorization(): IOperationAuthorizationService;
        createInterfaceAuthorization(): IOperationAuthorizationService;
        createDataProcessingAgreementAuthorization(): IOperationAuthorizationService;
    }

    export class AuthorizationServiceFactory implements IAuthorizationServiceFactory{
        static $inject = ["$http","userService"];
        constructor(
            private readonly $http: ng.IHttpService,
            private readonly userService: Services.IUserService) {

        }

        private createFor(entityType: string) {
            return new OperationAuthorizationService(this.$http, this.userService, entityType);
        }

        createOrganizationAuthorization(): IOperationAuthorizationService {
            return this.createFor("Organization");
        }

        createOrganizationUnitAuthorization(): IOperationAuthorizationService {
            return this.createFor("OrganizationUnit");
        }

        createSystemAuthorization(): IOperationAuthorizationService {
            return this.createFor("ItSystem");
        }

        createSystemUsageAuthorization(): IOperationAuthorizationService {
            return this.createFor("ItSystemUsage");
        }

        createReportAuthorization(): IOperationAuthorizationService {
            return this.createFor("Report");
        }

        createContractAuthorization(): IOperationAuthorizationService {
            return this.createFor("ItContract");
        }

        createProjectAuthorization(): IOperationAuthorizationService {
            return this.createFor("ItProject");
        }

        createInterfaceAuthorization(): IOperationAuthorizationService {
            return this.createFor("ItInterface");
        }

        createDataProcessingAgreementAuthorization(): IOperationAuthorizationService {
            return this.createFor("v1/data-processing-agreement");
        }
    }

    app.service("authorizationServiceFactory", AuthorizationServiceFactory);
}