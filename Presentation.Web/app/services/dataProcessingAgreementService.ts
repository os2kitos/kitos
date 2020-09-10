module Kitos.Services.DataProcessing {
    export interface IDataProcessingAgreementService {
        create(organizationId: number, name: string): angular.IPromise<IDataProcessingAgreementCreatedResult>;
        delete(dataProcessingAgreementId: number): angular.IPromise<IDataProcessingAgreementDeletedResult>;
        rename(dataProcessingAgreementId: number, name: string): angular.IPromise<IDataProcessingAgreementPatchResult>;
    }

    export interface IDataProcessingAgreementCreatedResult {
        createdObjectId : number;
    }


    export interface IDataProcessingAgreementDeletedResult {
        deleted: boolean;
        deletedObjectId: number;
        error: string;
    }

    export interface IDataProcessingAgreementPatchResult {
        modified: boolean;
        valueModified: string;
        error: string;
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

        public rename(dataProcessingAgreementId: number, name: string): angular.IPromise<IDataProcessingAgreementPatchResult> { 

            const payload  = {
                Value: name
            };

            return this
                .$http
                .patch<API.Models.IApiWrapper<any>>(this.getUriWithIdAndSuffix(dataProcessingAgreementId.toString(), "name"), payload).then(
                    response => {
                    return <IDataProcessingAgreementPatchResult>{
                        modified: true,
                        valueModified: name
                    };
                },
                error => {
                    return <IDataProcessingAgreementPatchResult>{
                        modified: false,
                        valueModified: name,
                        error: error.data.msg
                };
                }
            );
        }

        public delete(dataProcessingAgreementId: number): angular.IPromise<IDataProcessingAgreementDeletedResult> {

            return this
                .$http
                .delete<API.Models.IApiWrapper<any>>(this.getUri(dataProcessingAgreementId.toString()))
                .then(
                    response => {
                        return <IDataProcessingAgreementDeletedResult>{
                            deleted: true,
                            deletedObjectId: response.data.response.id,
                            error: "TODO"
                        };
                    },
                    error => {
                        return <IDataProcessingAgreementDeletedResult>{
                            deleted: false,
                            error: "TODO"
                        };
                    }
                );

        }

        public create(organizationId: number, name: string): angular.IPromise<IDataProcessingAgreementCreatedResult> {
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
            return this.getBaseUri() + `${suffix}`;
        }

        private getUriWithIdAndSuffix(id: string, suffix: string) {
            return this.getBaseUri() + `${id}/${suffix}`;
        }

        private getBaseUri() {
            return "api/v1/data-processing-agreement/";
        }
    }

    app.service("dataProcessingAgreementService", DataProcessingAgreementService);
}