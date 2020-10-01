module Kitos.Services {
    import IReference = Models.IReference;
    import BaseReference = Models.BaseReference;

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
            private readonly entityType: string,
            private readonly creationTypeFunc: Function) {
        }

        private createReferenceModel(
            entityId: number,
            title: string,
            externalReferenceId: string,
            url: string): BaseReference {
            let referenceModel = {
                Title: title,
                ExternalReferenceId: externalReferenceId,
                URL: url,
                Created: new Date()
            };
            referenceModel = this.creationTypeFunc(referenceModel, entityId);
            return referenceModel;
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

            const data = this.createReferenceModel(entityId, title, externalReferenceId, url);
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
        createSystemUsageReference(): IReferenceService;
        createContractReference(): IReferenceService;
        createProjectReference(): IReferenceService;
        createDpaReference(): IReferenceService;
    }

    export class ReferenceServiceFactory implements IReferenceServiceFactory {

        static $inject = ["$http"];
        constructor(private readonly $http: ng.IHttpService) {

        }

        private createFor(entityType: string, creationTypeFunc: Function) {
            return new ReferenceService(this.$http, entityType, creationTypeFunc);
        }

        createSystemReference(): IReferenceService {
            return this.createFor("itSystem", (reference, id) => {
                reference.ItSystem_Id = id;
                return reference;
            });
        }

        createSystemUsageReference(): IReferenceService {
            return this.createFor("itSystemUsage", (reference, id) => {
                reference.ItSystemUsage_Id = id;
                return reference;
            });
        }


        createContractReference(): IReferenceService {
            return this.createFor("itContract", (reference, id) => {
                reference.ItContract_Id = id;
                return reference;
            }); }

        createProjectReference(): IReferenceService {
            return this.createFor("itProject", (reference, id) => {
                reference.ItProject_Id = id;
                return reference;
            });
        }

        createDpaReference(): IReferenceService {
            return this.createFor("v1/data-processing-registration", (reference, id) => {
                reference.DataProcessingRegistration_Id = id;
                return reference;
            });
        }
    }

    app.service("referenceServiceFactory", ReferenceServiceFactory);
}