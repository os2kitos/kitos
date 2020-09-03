module Kitos.Services.DataProcessing {

    export interface IDataProcessingAgreementService {
        //TODO: Extend with type safe methods for getting and changing data
    }

    export class DataProcessingAgreementService implements IDataProcessingAgreementService {

        static $inject = ["$http", "notify"];

        constructor(private readonly $http: ng.IHttpService, private notify) {
        }

        private getUri(suffix: string) : string {
            return `api/v1/data-processing-agreement/${suffix}`;
        }
    }

    app.service("dataProcessingAgreementService", DataProcessingAgreementService);
}