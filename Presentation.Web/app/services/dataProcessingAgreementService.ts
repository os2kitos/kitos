module Kitos.Services.DataProcessing {
    import IApiWrapper = API.Models.IApiWrapper;

    export interface IDataProcessingAgreementService {
        create(organizationId: number, name: string): angular.IPromise<IDataProcessingAgreementCreatedResult>;
        //TODO: Extend with type safe methods for getting and changing data
    }

    export interface IDataProcessingAgreementCreatedResult {
        created: boolean;
        createdObjectId : number;
        error: string;
    }

    export class DataProcessingAgreementService implements IDataProcessingAgreementService {
        public create(organizationId: number, name: string): angular.IPromise<IDataProcessingAgreementCreatedResult> {
            const payload = {
                name: name,
                organizationId: organizationId
            };
            return this
                .$http
                .post<IApiWrapper<any>>(this.getUri(""), payload)
                .then(
                    response => {
                        return <IDataProcessingAgreementCreatedResult>{
                            created: true,
                            createdObjectId: response.data.response.id,
                            error: "TODO"
                        };
                    },
                    error => {
                        return <IDataProcessingAgreementCreatedResult>{
                            created: false,
                            error: "TODO"
                        };
                    }
                );
        }

        static $inject = ["$http", "notify"];

        constructor(private readonly $http: ng.IHttpService, private notify) {
        }

        private getUri(suffix: string) : string {
            return `api/v1/data-processing-agreement/${suffix}`;
        }
    }

    app.service("dataProcessingAgreementService", Kitos.Services.DataProcessing.DataProcessingAgreementService);
}