module Kitos.Services {
    import IReference = Models.IReference;

    export interface IReferenceService {
        getReference(referenceId: number): ng.IPromise<IReference>;
        createReference(entityId: number, title: string, externalReferenceId: string, url: string);
        deleteReference(referenceId: number, orgId: number);
        updateReference(referenceId: number, orgId: number, title: string, externalReferenceId: string, url: string);
        setOverviewReference(entityId: number, orgId: number, referenceId: number);
    }

    export class ReferenceService implements IReferenceService {
        constructor(
            private readonly $http: ng.IHttpService,
            private readonly entityType: string) {
        }

        private referenceBasePath = "api/Reference";

        private getBasePath() {
            return `api/${this.entityType}`;
        }

        getReference(referenceId: number) {
            return this.$http.get(`${this.referenceBasePath}/${referenceId}`)
                .then(success => {
                    var result = success.data as { msg: string, response: IReference }
                    return result.response;
                });
        }

        createReference(entityId: number,
            title: string,
            externalReferenceId: string,
            url: string) {
            const data = {
                ItSystem_Id: entityId,
                Title: title,
                ExternalReferenceId: externalReferenceId,
                URL: url,
                Created: new Date()
            };
            return this.$http.post(this.referenceBasePath, data);
        }

        updateReference(referenceId: number,
            orgId: number,
            title: string,
            externalReferenceId: string,
            url: string) {
            const data = {
                Title: title,
                ExternalReferenceId: externalReferenceId,
                URL: url
            };
            return this.$http.patch(`${this.referenceBasePath}/${referenceId}?organizationId=${orgId}`, data);
        }

        deleteReference(referenceId: number, orgId: number) {
            return this.$http.delete(`${this.referenceBasePath}/${referenceId}?organizationId=${orgId}`);
        }

        setOverviewReference(entityId: number, orgId: number, referenceId: number) {
            const data = {
                ReferenceId: referenceId
            };
            return this.$http.patch(`${this.getBasePath()}/${entityId}?organizationId=${orgId}`, data);
        }
    }

    export interface IReferenceServiceFactory {
        createSystemReference(): IReferenceService;
    }

    export class ReferenceServiceFactory implements IReferenceServiceFactory {
        static $inject = ["$http"];
        constructor(private readonly $http: ng.IHttpService) {

        }

        private createFor(entityType: string) {
            return new ReferenceService(this.$http, entityType);
        }

        createSystemReference(): IReferenceService {
            return this.createFor("itSystem");
        }
    }

    app.service("referenceServiceFactory", ReferenceServiceFactory);
}