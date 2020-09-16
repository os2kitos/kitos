module Kitos.Services.DataProcessing {

    export interface IDataProcessingAgreementService {
        create(organizationId: number, name: string): angular.IPromise<IDataProcessingAgreementCreatedResult>;
        delete(dataProcessingAgreementId: number): angular.IPromise<IDataProcessingAgreementDeletedResult>;
        rename(dataProcessingAgreementId: number, name: string): angular.IPromise<IDataProcessingAgreementPatchResult>;
        get(dataProcessingAgreementId: number): angular.IPromise<Models.DataProcessing.IDataProcessingAgreementDTO>;
        setMasterReference(dataProcessingAgreementId: number, referenceId: number): angular.IPromise<IDataProcessingAgreementPatchResult>;
    }

    export interface IDataProcessingAgreementCreatedResult {
        createdObjectId: number;
    }


    export interface IDataProcessingAgreementDeletedResult {
        deletedObjectId: number;
    }

    export interface IDataProcessingAgreementPatchResult {
        valueModifiedTo: string;
    }

    export class DataProcessingAgreementService implements IDataProcessingAgreementService {

        private handleServerError(error) {
            console.log("Request failed with:", error);
            let errorCategory: Models.Api.ApiResponseErrorCategory;
            switch (error.status) {
                case 400:
                    errorCategory = Models.Api.ApiResponseErrorCategory.BadInput;
                    break;
                case 404:
                    errorCategory = Models.Api.ApiResponseErrorCategory.NotFound;
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

        rename(dataProcessingAgreementId: number, name: string): angular.IPromise<IDataProcessingAgreementPatchResult> {

            const payload = {
                Value: name
            };

            return this
                .$http
                .patch<API.Models.IApiWrapper<any>>(this.getUriWithIdAndSuffix(dataProcessingAgreementId.toString(), "name"), payload)
                .then(
                    response => {
                        return <IDataProcessingAgreementPatchResult>{
                            valueModifiedTo: name,
                        };
                    },
                    error => this.handleServerError(error)
                );
        }

        update(agreement: Models.DataProcessing.IDataProcessingAgreementDTO): angular.IPromise<IDataProcessingAgreementPatchResult> {
            return this
                .$http
                .patch<API.Models.IApiWrapper<any>>(this.getUri(""), agreement)
                .then(
                    response => {
                        return <IDataProcessingAgreementPatchResult>{
                            valueModifiedTo: response.data.response.id,
                        };
                    },
                    error => this.handleServerError(error)
                );
        }

        delete(dataProcessingAgreementId: number): angular.IPromise<IDataProcessingAgreementDeletedResult> {

            return this
                .$http
                .delete<API.Models.IApiWrapper<any>>(this.getUri(dataProcessingAgreementId.toString()))
                .then(
                    response => {
                        return <IDataProcessingAgreementDeletedResult>{
                            deletedObjectId: response.data.response.id,
                        };
                    },
                    error => this.handleServerError(error)
                );

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


        get(dataProcessingAgreementId: number): angular.IPromise<Models.DataProcessing.IDataProcessingAgreementDTO> {
            return this
                .$http
                .get<API.Models.IApiWrapper<any>>(this.getUri(dataProcessingAgreementId.toString()))
                .then(
                    result => {
                        var response = result.data as { response: Models.DataProcessing.IDataProcessingAgreementDTO }
                        return response.response;
                    },
                    error => this.handleServerError(error)
                );
        }

        setMasterReference(dataProcessingAgreementId: number, referenceId: number): angular.IPromise<IDataProcessingAgreementPatchResult> {

            const referenceService = new ReferenceServiceFactory(this.$http).createDpaReference();

            return referenceService.getReference(referenceId).then(ref => {
                return this
                    .$http
                    .patch<API.Models.IApiWrapper<any>>(
                        this.getUriWithIdAndSuffix(dataProcessingAgreementId.toString(), "reference"), ref)
                    .then(
                        response => {
                            return <IDataProcessingAgreementPatchResult>{
                                valueModifiedTo: ref.Title,
                            };
                        },
                        error => this.handleServerError(error)
                    );
            });
        }

        static $inject = ["$http"];

        constructor(private readonly $http: ng.IHttpService) {
        }

        private getUri(suffix: string): string {
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