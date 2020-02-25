module Kitos.Services {

    export interface IReferenceService {
        deleteReference(referenceId: number, orgId: number);
    }

    export class ReferenceService implements IReferenceService{

        static $inject = ["$http"];
        constructor(private readonly $http: ng.IHttpService) {
        }

        deleteReference(referenceId: number, orgId: number) {
            return this.$http.delete(`api/Reference/${referenceId}?organizationId=${orgId}`);
        }
    }

    app.service("systemRelationService", ReferenceService);
}