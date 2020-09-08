module Kitos.Services.DataProcessing {
    export interface IDataProcessingAgreementService {
        create(organizationId: number, name: string): angular.IPromise<IDataProcessingAgreementCreatedResult>;
        //TODO: Extend with type safe methods for getting and changing data
    }

    export interface IDataProcessingAgreementCreatedResult {
        createdObjectId : number;
    }

    export class DataProcessingAgreementService implements IDataProcessingAgreementService {

        private handleServerError(error) {
            console.log("Request failed with:", error);
            let errorCategory: Models.Api.ApiResponseErrorCategory;
            switch (error.status) {
            case 400:
                errorCategory = Models.Api.ApiResponseErrorCategory.BadInput;
                break;
            case 409:
                errorCategory = Models.Api.ApiResponseErrorCategory.Conflict;
                break;
            case 500:
                errorCategory = Models.Api.ApiResponseErrorCategory.ServerError;
                break;
            default:
                errorCategory = Models.Api.ApiResponseErrorCategory.UnknownError;
            }
            throw errorCategory;
        }

        create(organizationId: number, name: string): angular.IPromise<IDataProcessingAgreementCreatedResult> {
            const payload = {
                name: name,
                organizationId: organizationId
            };
            return this
                .$http
                .post<API.Models.IApiWrapper<any>>(this.getUri(""), payload)
                .then(
                    response => {
                        return <IDataProcessingAgreementCreatedResult>{
                            createdObjectId: response.data.response.id
                        };
                    },
                    error => this.handleServerError(error)
                );
        }

        static $inject = ["$http"];

        constructor(private readonly $http: ng.IHttpService) {
        }

        private getUri(suffix: string) : string {
            return `api/v1/data-processing-agreement/${suffix}`;
        }
    }

    app.service("dataProcessingAgreementService", DataProcessingAgreementService);
}