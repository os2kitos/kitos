module Kitos.Services {

    export interface IReferenceService {
        getReference(referenceId: number);
        createReference(entityId: number, title: string, externalReferenceId: string, url: string, display: any);
        deleteReference(referenceId: number, orgId: number);
        patchReference(referenceId: number, orgId: number, title: string, externalReferenceId: string, url: string, display: any);
        setOverviewReference(entityId: number, orgId: number, referenceId: number);
    }

    export class ReferenceService implements IReferenceService{
        constructor(
            private readonly $http: ng.IHttpService,
            private readonly entityType: string) {

        }

        private getBasePath() {
            return `api/${this.entityType}`;
        }

        getReference(referenceId: number) {
            return this.$http.get(`api/Reference/${referenceId}`);
        }

        createReference(entityId: number, title: string, externalReferenceId: string, url: string, display) {
            const data = {
                ItSystem_Id: entityId,
                Title: title,
                ExternalReferenceId: externalReferenceId,
                URL: url,
                Display: display,
                Created: new Date()
            };
            return this.$http.post("api/Reference", data);
        }

        patchReference(referenceId: number,
            orgId: number,
            title: string,
            externalReferenceId: string,
            url: string,
            display) {
            const data = {
                Title: title,
                ExternalReferenceId: externalReferenceId,
                URL: url,
                Display: display
            };
            return this.$http.patch(`api/Reference/${referenceId}?organizationId=${orgId}`, data);
        }

        deleteReference(referenceId: number, orgId: number) {
            return this.$http.delete(`api/Reference/${referenceId}?organizationId=${orgId}`);
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